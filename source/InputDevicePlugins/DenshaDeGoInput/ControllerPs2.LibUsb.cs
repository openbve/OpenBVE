﻿//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2021, Marc Riera, The OpenBVE Project
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

using System;
using System.Collections.Generic;
using System.Threading;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using OpenBveApi;
using OpenBveApi.Interface;

namespace DenshaDeGoInput
{
	public partial class ControllerPs2
	{
		/// <summary>
		/// Dictionary containing the supported USB controllers
		/// </summary>
		internal static Dictionary<Guid, UsbController> supportedUsbControllers = new Dictionary<Guid, UsbController>();

		/// <summary>
		/// The thread which spins to poll for LibUsb input
		/// </summary>
		internal static Thread LibUsbThread;

		/// <summary>
		/// The control variable for the LibUsb input thread
		/// </summary>
		internal static bool LibUsbShouldLoop = true;

		/// <summary>
		/// The setup packet needed to send data to the controller.
		/// </summary>
		internal static UsbSetupPacket setupPacket = new UsbSetupPacket(0x40, 0x09, 0x0301, 0x0000, 0x0008);

		internal static void LibUsbLoop()
		{
			while (LibUsbShouldLoop && !DenshaDeGoInput.LibUsbIssue)
			{
				// First, let's check which USB devices are connected
				CheckConnectedControllers();

				// If the current controller is a supported controller and is connected, poll it for input
				if (supportedUsbControllers.ContainsKey(InputTranslator.ActiveControllerGuid) && supportedUsbControllers[InputTranslator.ActiveControllerGuid].IsConnected)
				{
					supportedUsbControllers[InputTranslator.ActiveControllerGuid].Poll();
				}
			}

			foreach (var controller in supportedUsbControllers.Values)
			{
				controller.Unload();
			}
		}

		/// <summary>
		/// Checks the connection status of supported LibUsb controllers.
		/// </summary>
		internal static void CheckConnectedControllers()
		{
			if (DenshaDeGoInput.LibUsbIssue)
			{
				return;
			}
			try
			{
				foreach (UsbController controller in supportedUsbControllers.Values)
				{
					if (controller.ControllerDevice == null)
					{
						// The device is not configured, try to find it
						controller.ControllerDevice = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(controller.VendorID, controller.ProductID));
					}
					if (controller.ControllerDevice == null)
					{
						// The controller is not connected
						controller.IsConnected = false;
					}
					else
					{
						// The controller is connected
						controller.IsConnected = true;
						if (controller.ControllerReader == null)
						{
							// Open endpoint reader if necessary
							controller.ControllerReader = controller.ControllerDevice.OpenEndpointReader(ReadEndpointID.Ep01);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.StackTrace);
				if (DenshaDeGoInput.CurrentHost.SimulationState == SimulationState.Running)
				{
					DenshaDeGoInput.CurrentHost.AddMessage(MessageType.Error, false, "The DenshaDeGo! Input Plugin encountered a critical error whilst attempting to update the connected controller list.");
				}
				//LibUsb isn't working right
				DenshaDeGoInput.LibUsbIssue = true;
			}
		}
	}
}
