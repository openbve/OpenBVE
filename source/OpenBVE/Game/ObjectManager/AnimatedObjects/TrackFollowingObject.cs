using System;
using LibRender;
using OpenBve.RouteManager;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using static LibRender.CameraProperties;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		internal class TrackFollowingObject : WorldObject
		{
			private readonly HostInterface currentHost;
			/// <summary>The actual animated object</summary>
			internal AnimatedObject Object;
			/// <summary>The signalling section the object refers to (Only relevant for objects placed using Track.Sig</summary>
			internal int SectionIndex;

			internal TrackFollower FrontAxleFollower;
			internal TrackFollower RearAxleFollower;
			internal double FrontAxlePosition;
			internal double RearAxlePosition;
#pragma warning disable 0649
//TODO: Track following objects currently do not take into account toppling or cant, reserved for future use
			internal double CurrentRollDueToTopplingAngle;
			internal double CurrentRollDueToCantAngle;
#pragma warning restore 0649       

			internal TrackFollowingObject(HostInterface Host)
			{
				currentHost = Host;
			}

			public override void Update(AbstractTrain NearestTrain, double TimeElapsed, bool ForceUpdate, bool Visible)
			{
				
				if (Visible | ForceUpdate)
				{
					if (Object.SecondsSinceLastUpdate >= Object.RefreshRate | ForceUpdate)
					{
						double timeDelta = Object.SecondsSinceLastUpdate + TimeElapsed;
						Object.SecondsSinceLastUpdate = 0.0;

						if (base.Visible)
						{
							//Calculate the distance travelled
							double delta = UpdateTrackFollowerScript(false, NearestTrain, NearestTrain == null ? 0 : NearestTrain.DriverCar, SectionIndex, TrackPosition, Position, true, timeDelta);
							//Update the front and rear axle track followers
							FrontAxleFollower.UpdateAbsolute((TrackPosition + FrontAxlePosition) + delta, true, true);
							RearAxleFollower.UpdateAbsolute((TrackPosition + RearAxlePosition) + delta, true, true);
							//Update the base object position
							FrontAxleFollower.UpdateWorldCoordinates(false);
							RearAxleFollower.UpdateWorldCoordinates(false);
							UpdateObjectPosition();
						}
						//Update the actual animated object- This must be done last in case the user has used Translation or Rotation
						Object.Update(false, NearestTrain, NearestTrain == null ? 0 : NearestTrain.DriverCar, SectionIndex, FrontAxleFollower.TrackPosition, FrontAxleFollower.WorldPosition, Direction, Up, Side, true, true, timeDelta, true);
					}
					else
					{
						Object.SecondsSinceLastUpdate += TimeElapsed;
					}
					if (!base.Visible)
					{
						currentHost.ShowObject(Object.internalObject, ObjectType.Dynamic);
						base.Visible = true;
					}
				}
				else
				{
					Object.SecondsSinceLastUpdate += TimeElapsed;
					if (base.Visible)
					{
						currentHost.HideObject(ref Object.internalObject);
						base.Visible = false;
					}
				}
			}

			/// <summary>Updates the position and rotation of an animated object which follows a track</summary>
			private void UpdateObjectPosition()
			{
				//Get vectors
				Direction = new Vector3(FrontAxleFollower.WorldPosition - RearAxleFollower.WorldPosition);
				{
					double t = 1.0 / Direction.Norm();
					Direction *= t;
					t = 1.0 / Math.Sqrt(Direction.X * Direction.X + Direction.Z * Direction.Z);
					double ex = Direction.X * t;
					double ez = Direction.Z * t;
					Side = new Vector3(ez, 0.0, -ex);
					Up = Vector3.Cross(Direction, Side);
				}

				// apply position due to cant/toppling
				{
					double a = CurrentRollDueToTopplingAngle + CurrentRollDueToCantAngle;
					double x = Math.Sign(a) * 0.5 * FrontAxleFollower.RailGauge * (1.0 - Math.Cos(a));
					double y = Math.Abs(0.5 * FrontAxleFollower.RailGauge * Math.Sin(a));
					Vector3 c = Side * x + Up * y;

					FrontAxleFollower.WorldPosition += c;
					RearAxleFollower.WorldPosition += c;
				}
				// apply rolling
				{
					double a = CurrentRollDueToTopplingAngle - CurrentRollDueToCantAngle;
					double cosa = Math.Cos(a);
					double sina = Math.Sin(a);
					Side.Rotate(Direction, cosa, sina);
					Up.Rotate(Direction, cosa, sina);
				}
			}

			private double UpdateTrackFollowerScript(bool IsPartOfTrain, AbstractTrain Train, int CarIndex, int currentSectionIndex, double currentTrackPosition, Vector3 WorldPosition, bool UpdateFunctions, double TimeElapsed)
			{
				double x = 0.0;
				if (Object.TrackFollowerFunction != null)
				{
					if (UpdateFunctions)
					{
						x = Object.TrackFollowerFunction.Perform(Train, CarIndex, WorldPosition, currentTrackPosition, currentSectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
					}
					else
					{
						x = Object.TrackFollowerFunction.LastResult;
					}
				}
				return x;
			}
		}
	}
}
