﻿//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2022, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using OpenBveApi.Runtime;
using OpenBveApi.Trains;

namespace TrainManager.SafetySystems
{
	/// <summary>An AI to control the legacy Win32 UKDt plugin</summary>
	internal class UKDtAI : PluginAI
	{
		/// <summary>Whether the overcurrent trip has occurred</summary>
		private bool overCurrentTrip;
		/// <summary>The speed at which the overcurrent trip occurred</summary>
		private double overCurrentSpeed;
		/// <summary>The notch at which the overcurrent trip occurred</summary>
		private int overCurrentNotch;
		/// <summary>Timer to reset vigilance when necessary</summary>
		private double vigilanceTimer;

		internal UKDtAI(Plugin plugin)
		{
			Plugin = plugin;
			currentStep = 0;
			nextPluginAction = 0;
			vigilanceTimer = 0;
		}

		internal override void Perform(AIData data)
		{
			if (Plugin.Train.CurrentSpeed != 0 && currentStep == 0)
			{
				// ai asked to take over at speed, skip startup sequence
				currentStep = 100;
			}

			switch (currentStep)
			{
				case 0:
					// start of startup sequence- start by bringing up TPWS / AWS
					data.Handles.Reverser = 1;
					data.Response = AIResponse.Long;
					currentStep++;
					return;
				case 1:
					data.Handles.Reverser = 0;
					data.Response = AIResponse.Long;
					currentStep++;
					return;
				case 2:
					if (Plugin.Sound[2] == 0)
					{
						data.Response = AIResponse.Medium;
						currentStep++;
					}
					return;
				case 3:
					Plugin.KeyDown(VirtualKeys.A1);
					data.Response = AIResponse.Medium;
					currentStep++;
					return;
				case 4:
					Plugin.KeyUp(VirtualKeys.A1);
					data.Response = AIResponse.Medium;
					// check engine state
					if (Plugin.Panel[5] == 2)
					{
						// engine stopped, continue with startup
						currentStep++;
					}
					else
					{
						// startup test complete
						currentStep = 100;
					}
					return;
				case 5:
					Plugin.KeyDown(VirtualKeys.D);
					data.Response = AIResponse.Long;
					currentStep++;
					return;
				case 6:
					currentStep++;
					return;
				case 7:
					Plugin.KeyUp(VirtualKeys.D);
					data.Response = AIResponse.Medium;
					currentStep = 100;
					return;
			}

			if (Plugin.Panel[5] == 3)
			{
				// engine either stalled on start, or has been stopped by user
				currentStep = 5;
			}

			if (Plugin.Panel[51] == 1)
			{
				/*
				 * Over current has tripped
				 * Let's back off to N and drop the max notch by 1
				 *
				 * Repeat until we move off properly
				 * NOTE: UKDT does have an ammeter, but we'll cheat this way, to
				 * avoid having to configure the max on a per-train basis
				 */
				if (!overCurrentTrip)
				{
					overCurrentSpeed = Plugin.Train.CurrentSpeed;
					overCurrentNotch = data.Handles.PowerNotch - 1;
					data.Handles.PowerNotch = 0;
					data.Response = AIResponse.Long;
					overCurrentTrip = true;
					return;
				}
				data.Response = AIResponse.Long;
				return;
			}

			overCurrentTrip = false;
			if (overCurrentSpeed != double.MaxValue)
			{
				if (Plugin.Train.CurrentSpeed < overCurrentSpeed + 10)
				{
					data.Handles.PowerNotch = overCurrentNotch;
				}
				else
				{
					overCurrentSpeed = double.MaxValue;
				}
			}

			if (Plugin.Sound[2] == 0)
			{
				//AWS horn active, so wait a sec before triggering cancel
				switch (currentStep)
				{
					case 100:
						data.Response = AIResponse.Medium;
						currentStep++;
						return;
					case 101:
						Plugin.KeyDown(VirtualKeys.A1);
						data.Response = AIResponse.Medium;
						currentStep++;
						return;
				}
			}

			if (Plugin.Sound[3] == 0)
			{
				//Vigilance alarm- driver to increase brakes to max
				if (data.Handles.BrakeNotch != Plugin.Train.Handles.Brake.MaximumNotch)
				{
					data.Handles.BrakeNotch++;
					data.Response = AIResponse.Short;
					return;
				}
				if (data.Handles.PowerNotch != 0)
				{
					data.Handles.PowerNotch--;
					data.Response = AIResponse.Short;
					return;
				}
				//Wait for train to stop
				if (Plugin.Train.CurrentSpeed != 0)
				{
					data.Response = AIResponse.Short;
					return;
				}
				//Reset alarm
				switch (currentStep)
				{
					case 100:
						data.Response = AIResponse.Medium;
						currentStep++;
						return;
					case 101:
						Plugin.KeyDown(VirtualKeys.A2);
						data.Response = AIResponse.Medium;
						currentStep+= 2;
						return;
				}
			}
			
			if (currentStep == 102 || currentStep == 103)
			{
				//Raise the cancel key
				Plugin.KeyUp(currentStep == 102 ? VirtualKeys.A1 : VirtualKeys.A2);
				data.Response = AIResponse.Medium;
				currentStep = 100;
				return;
			}

			/*
			 * Assume that with a brightness value below 180 we want night headlights
			 * Further assume that the driver only sets these at the initial station once
			 */
			if (!lightsSet)
			{
				float currentBrightness = Plugin.Train.Cars[Plugin.Train.DriverCar].Brightness.CurrentBrightness(TrainManagerBase.Renderer.Lighting.DynamicCabBrightness, 0.0);
				switch (Plugin.Panel[20])
				{
					case 0:
						//off
						Plugin.KeyDown(VirtualKeys.G);
						Plugin.KeyUp(VirtualKeys.G);
						data.Response = AIResponse.Medium;
						return;
					case 1:
						//day
						if (currentBrightness < 180)
						{
							Plugin.KeyDown(VirtualKeys.G);
							Plugin.KeyUp(VirtualKeys.G);
							data.Response = AIResponse.Medium;
						}
						else
						{
							lightsSet = true;
						}
						return;
					case 2:
						//marker
						Plugin.KeyDown(VirtualKeys.G);
						Plugin.KeyUp(VirtualKeys.G);
						data.Response = AIResponse.Medium;
						return;
					case 3:
						//night
						if (currentBrightness > 180)
						{
							Plugin.KeyDown(VirtualKeys.G);
							Plugin.KeyUp(VirtualKeys.G);
							data.Response = AIResponse.Medium;
						}
						else
						{
							lightsSet = true;
						}
						return;
				}
			}
			//Handle DRA
			if (Plugin.Train.StationState == TrainStopState.Boarding)
			{
				if (Plugin.Panel[13] == 0)
				{
					Plugin.KeyDown(VirtualKeys.S);
					Plugin.KeyUp(VirtualKeys.S);
					data.Response = AIResponse.Short;
					return;
				}
			}
			else
			{
				if (Plugin.Panel[13] == 1)
				{
					Plugin.KeyDown(VirtualKeys.S);
					Plugin.KeyUp(VirtualKeys.S);
					data.Response = AIResponse.Short;
					return;
				}
			}
			//UKDT fitted trains generally use the tail lights toggle for something else dummy (ETS etc.) which isn't directly relevant to the AI or the external appearance, so do nothing here

			if (TrainManagerBase.currentHost.InGameTime > nextPluginAction)
			{
				//If nothing else has happened recently, blip power / brake (some trains don't like using the vigilance reset key to keep going)
				if (data.Handles.BrakeNotch == 0)
				{
					if (data.Handles.PowerNotch > 0)
					{
						data.Handles.PowerNotch--;
					}
					else
					{
						data.Handles.BrakeNotch++;
					}
				}
				else
				{
					if (data.Handles.BrakeNotch < Plugin.Train.Handles.Brake.MaximumDriverNotch)
					{
						data.Handles.BrakeNotch++;
					}
					else
					{
						// this actually does nothing, but satisfies the plugin
						data.Handles.PowerNotch++;
					}
				}
				data.Response = AIResponse.Short;
				nextPluginAction = TrainManagerBase.currentHost.InGameTime + 20.0;
				return;
			}

			//Count number of shown raindrops
			int numShownRaindrops = 0;
			for (int i = 200; i < 250; i++)
			{
				if (Plugin.Panel[i] == 1)
				{
					numShownRaindrops++;
				}
			}
			//Greater than 10 drops, always clear the screen
			bool shouldWipe = numShownRaindrops > 10;

			switch (Plugin.Panel[198])
			{
				case 0:
					if (currentRainIntensity > 30 || shouldWipe)
					{
						Plugin.KeyDown(VirtualKeys.B1);
						Plugin.KeyUp(VirtualKeys.B1);
						data.Response = AIResponse.Short;
					}
					return;
				case 1:
					if (currentRainIntensity > 45)
					{
						Plugin.KeyDown(VirtualKeys.B1);
						Plugin.KeyUp(VirtualKeys.B1);
						data.Response = AIResponse.Short;
					}
					else
					{
						Plugin.KeyDown(VirtualKeys.B2);
						Plugin.KeyUp(VirtualKeys.B2);
						data.Response = AIResponse.Short;
					}
					return;
				case 2:
					if (currentRainIntensity < 60)
					{
						Plugin.KeyDown(VirtualKeys.B2);
						Plugin.KeyUp(VirtualKeys.B2);
						data.Response = AIResponse.Short;
					}
					return;
			}

			vigilanceTimer += data.TimeElapsed;
			if (vigilanceTimer > 20000 && Plugin.Train.CurrentSpeed != 0)
			{
				vigilanceTimer = 0;
				if (data.Handles.BrakeNotch > 0)
				{
					if (data.Handles.BrakeNotch < Plugin.Train.Handles.Brake.MaximumNotch)
					{
						// quick further blip on the brakes to satisfy vigilance
						data.Handles.BrakeNotch++;
					}
					else
					{
						// can't increase brake notch any further, so blip power, although it does nothing
						data.Handles.PowerNotch++;
					}
					data.Response = AIResponse.Short;
					return;
				}
				if (data.Handles.PowerNotch > 0)
				{
					// drop off the power a sec
					data.Handles.PowerNotch--;
					data.Response = AIResponse.Short;
					return;
				}

				if (data.Handles.PowerNotch == 0)
				{
					// Running at appropriate speed with no power, so tap brakes a sec to control + satisfy vigilance
					data.Handles.BrakeNotch++;
					data.Response = AIResponse.Short;
					return;
				}
			}
		}

		public override void BeginJump(InitializationModes mode)
		{
			overCurrentTrip = false;
			if (mode == InitializationModes.OffEmergency)
			{
				currentStep = 0;
			}
			else
			{
				currentStep = 100;
			}
		}

		public override void EndJump()
		{

		}

		public override void SetBeacon(BeaconData beacon)
		{
			if (beacon.Type == 21)
			{
				currentRainIntensity = beacon.Optional;
			}
		}
	}
}
