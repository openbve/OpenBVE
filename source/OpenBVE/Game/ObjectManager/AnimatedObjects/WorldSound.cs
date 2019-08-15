﻿using System;
using LibRender;
using OpenBve.RouteManager;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using OpenBveApi.World;
using SoundManager;
using static LibRender.CameraProperties;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		/// <summary>Represents a world sound attached to an .animated file</summary>
		internal class WorldSound : WorldObject
		{
			/// <summary>The sound buffer to play</summary>
			internal SoundBuffer Buffer;
			/// <summary>The sound source for this file</summary>
			internal SoundSource Source;
			/// <summary>The pitch to play the sound at</summary>
			internal double currentPitch = 1.0;
			/// <summary>The volume to play the sound at it's origin</summary>
			internal double currentVolume = 1.0;
			/// <summary>The track position</summary>
			internal double currentTrackPosition = 0;
			/// <summary>The track follower used to hold/ move the sound</summary>
			internal TrackFollower Follower;
			/// <summary>The function script controlling the sound's movement along the track, or a null reference</summary>
			internal FunctionScript TrackFollowerFunction;
			/// <summary>The function script controlling the sound's volume, or a null reference</summary>
			internal FunctionScript VolumeFunction;
			/// <summary>The function script controlling the sound's pitch, or a null reference</summary>
			internal FunctionScript PitchFunction;

			internal WorldSound()
			{
				Radius = 25.0;
			}

			internal void CreateSound(Vector3 position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, double trackPosition)
			{
				int a = AnimatedWorldObjectsUsed;
				if (a >= AnimatedWorldObjects.Length)
				{
					Array.Resize<WorldObject>(ref AnimatedWorldObjects, AnimatedWorldObjects.Length << 1);
				}
				WorldSound snd = new WorldSound
				{
					Buffer = this.Buffer,
					//Must clone the vector, not pass the reference
					Position = new Vector3(position),
					Follower =  new TrackFollower(CurrentRoute.Tracks),
					currentTrackPosition = trackPosition
				};
				snd.Follower.UpdateAbsolute(trackPosition, true, true);
				if (this.TrackFollowerFunction != null)
				{
					snd.TrackFollowerFunction = this.TrackFollowerFunction.Clone();
				}
				AnimatedWorldObjects[a] = snd;
				AnimatedWorldObjectsUsed++;
			}

			public override void Update(AbstractTrain NearestTrain, double TimeElapsed, bool ForceUpdate, bool Visible)
			{
				if (Visible | ForceUpdate)
				{
					if (Game.MinimalisticSimulation || TimeElapsed > 0.05)
					{
						return;
					}
					
					if (this.TrackFollowerFunction != null)
					{

						double delta = this.TrackFollowerFunction.Perform(NearestTrain, NearestTrain == null ? 0 : NearestTrain.DriverCar, this.Position, this.Follower.TrackPosition, 0, false, TimeElapsed, 0);
						this.Follower.UpdateAbsolute(this.currentTrackPosition + delta, true, true);
						this.Follower.UpdateWorldCoordinates(false);
					}
					if (this.VolumeFunction != null)
					{
						this.currentVolume = this.VolumeFunction.Perform(NearestTrain, NearestTrain == null ? 0 : NearestTrain.DriverCar, this.Position, this.Follower.TrackPosition, 0, false, TimeElapsed, 0);
					}
					if (this.PitchFunction != null)
					{
						this.currentPitch = this.PitchFunction.Perform(NearestTrain, NearestTrain == null ? 0 : NearestTrain.DriverCar, this.Position, this.Follower.TrackPosition, 0, false, TimeElapsed, 0);
					}
					if (this.Source != null)
					{
						this.Source.Pitch = this.currentPitch;
						this.Source.Volume = this.currentVolume;
					}
					//Buffer should never be null, but check it anyways
					if (!Program.Sounds.IsPlaying(Source) && Buffer != null)
					{
						Source = Program.Sounds.PlaySound(Buffer, 1.0, 1.0, Follower.WorldPosition + Position, this, true);
					}
				}
				else
				{
					if (Program.Sounds.IsPlaying(Source))
					{
						Program.Sounds.StopSound(Source);
					}
				}
				
			}
		}
	}
}
