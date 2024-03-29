using System;
using System.Linq;
using System.Windows.Forms;
using OpenBveApi.Math;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Power;
using TrainManager.SafetySystems;


namespace TrainEditor {
	internal static class TrainDat {

		// data structures

		// acceleration
		/// <summary>The Acceleration section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Acceleration {
			
			internal BveAccelerationCurve[] Entries;
			internal Acceleration() {
				const int n = 8;
				this.Entries = new BveAccelerationCurve[n];
				for (int i = 0; i < n; i++)
				{
					this.Entries[i] = new BveAccelerationCurve
					{
						StageZeroAcceleration = 1.0,
						StageOneAcceleration = 1.0,
						StageOneSpeed = 25.0,
						StageTwoSpeed = 25.0,
						StageTwoExponent = 1.0
					};
				}
			}
		}
		
		// performance
		/// <summary>The Performance section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Performance {
			internal double Deceleration;
			internal double CoefficientOfStaticFriction;
			internal double CoefficientOfRollingResistance;
			internal double AerodynamicDragCoefficient;
			internal Performance() {
				this.Deceleration = 1.0;
				this.CoefficientOfStaticFriction = 0.35;
				this.CoefficientOfRollingResistance = 0.0025;
				this.AerodynamicDragCoefficient = 1.2;
			}
		}
		
		// delay
		/// <summary>The Delay section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Delay {
			internal double[] DelayPowerUp;
			internal double[] DelayPowerDown;
			internal double[] DelayBrakeUp;
			internal double[] DelayBrakeDown;
			internal double[] DelayLocoBrakeUp;
			internal double[] DelayLocoBrakeDown;
			internal double ElectricBrakeDelayUp;
			internal double ElectricBrakeDelayDown;
			internal Delay() {
				this.DelayPowerUp = new[] { 0.0, 0.0, 0.0 , 0.0, 0.0, 0.0, 0.0, 0.0  };
				this.DelayPowerDown = new[] { 0.0, 0.0, 0.0 , 0.0, 0.0, 0.0, 0.0, 0.0  };
				this.DelayBrakeUp = new[] { 0.0, 0.0, 0.0 , 0.0, 0.0, 0.0, 0.0, 0.0  };
				this.DelayBrakeDown = new[] { 0.0, 0.0, 0.0 , 0.0, 0.0, 0.0, 0.0, 0.0  };
				this.DelayLocoBrakeUp = new[] { 0.0, 0.0, 0.0 , 0.0, 0.0, 0.0, 0.0, 0.0  };
				this.DelayLocoBrakeDown = new[] { 0.0, 0.0, 0.0 , 0.0, 0.0, 0.0, 0.0, 0.0  };
			}
		}
		
		// move
		/// <summary>The Move section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Move {
			internal double JerkPowerUp;
			internal double JerkPowerDown;
			internal double JerkBrakeUp;
			internal double JerkBrakeDown;
			internal double BrakeCylinderUp;
			internal double BrakeCylinderDown;
			internal Move() {
				this.JerkPowerUp = 1000.0;
				this.JerkPowerDown = 1000.0;
				this.JerkBrakeUp = 1000.0;
				this.JerkBrakeDown = 1000.0;
				this.BrakeCylinderUp = 300.0;
				this.BrakeCylinderDown = 200.0;
			}
		}
		
		// brake
		/// <summary>The Brake section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Brake {
		
			internal enum LocoBrakeTypes {
				NotFitted = 0,
				NotchedAirBrake = 1,
				AutomaticAirBrake = 2
			}
			
			internal BrakeSystemType BrakeType;
			internal LocoBrakeTypes LocoBrakeType;
			internal EletropneumaticBrakeType BrakeControlSystem;
			internal double BrakeControlSpeed;
			internal Brake() {
				this.BrakeType = BrakeSystemType.ElectromagneticStraightAirBrake;
				this.LocoBrakeType = LocoBrakeTypes.NotFitted;
				this.BrakeControlSystem = EletropneumaticBrakeType.None;
				this.BrakeControlSpeed = 0.0;
			}
		}
		
		// pressure
		/// <summary>The Pressure section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Pressure {
			internal double BrakeCylinderServiceMaximumPressure;
			internal double BrakeCylinderEmergencyMaximumPressure;
			internal double MainReservoirMinimumPressure;
			internal double MainReservoirMaximumPressure;
			internal double BrakePipeNormalPressure;
			internal Pressure() {
				this.BrakeCylinderServiceMaximumPressure = 480.0;
				this.BrakeCylinderEmergencyMaximumPressure = 480.0;
				this.MainReservoirMinimumPressure = 690.0;
				this.MainReservoirMaximumPressure = 780.0;
				this.BrakePipeNormalPressure = 490.0;
			}
		}
		
		// handle
		/// <summary>The Handle section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Handle {
			internal HandleType HandleType;
			internal int PowerNotches;
			internal int BrakeNotches;
			internal int PowerNotchReduceSteps;
			internal EbHandleBehaviour HandleBehaviour;
			internal LocoBrakeType LocoBrake;
			internal int LocoBrakeNotches;
			internal int DriverPowerNotches;
			internal int DriverBrakeNotches;
			internal Handle() {
				this.HandleType = HandleType.TwinHandle;
				this.PowerNotches = 8;
				this.BrakeNotches = 8;
				this.PowerNotchReduceSteps = 0;
				this.LocoBrakeNotches = 0;
				this.HandleBehaviour = EbHandleBehaviour.NoAction;
				this.LocoBrake = LocoBrakeType.Combined;
				this.DriverPowerNotches = 8;
				this.DriverBrakeNotches = 8;
			}
		}
		
		// cab
		/// <summary>The Cab section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Cab
		{
			internal Vector3 Driver;
			internal double DriverCar;
			internal Cab() {
				this.DriverCar = 0;
			}
		}

		
		// car
		/// <summary>The Car section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Car {
			internal double MotorCarMass;
			internal int NumberOfMotorCars;
			internal double TrailerCarMass;
			internal int NumberOfTrailerCars;
			internal double LengthOfACar;
			internal bool FrontCarIsAMotorCar;
			internal double WidthOfACar;
			internal double HeightOfACar;
			internal double CenterOfGravityHeight;
			internal double ExposedFrontalArea;
			internal double UnexposedFrontalArea;
			internal Car() {
				this.MotorCarMass = 40.0;
				this.NumberOfMotorCars = 1;
				this.TrailerCarMass = 40.0;
				this.NumberOfTrailerCars = 1;
				this.LengthOfACar = 20.0;
				this.FrontCarIsAMotorCar = false;
				this.WidthOfACar = 2.6;
				this.HeightOfACar = 3.2;
				this.CenterOfGravityHeight = 1.5;
				this.ExposedFrontalArea = 5.0;
				this.UnexposedFrontalArea = 1.6;
			}
		}
		
		// device
		/// <summary>The Device section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Device {
			internal AtsModes Ats;
			internal AtcModes Atc;
			internal bool Eb;
			internal bool ConstSpeed;
			internal bool HoldBrake;
			internal ReadhesionDeviceType ReAdhesionDevice;
			internal double LoadCompensatingDevice;
			internal PassAlarmType PassAlarm;
			internal DoorMode DoorOpenMode;
			internal DoorMode DoorCloseMode;
			internal double DoorWidth;
			internal double DoorMaxTolerance;
			internal Device() {
				this.Ats = AtsModes.AtsSn;
				this.Atc = AtcModes.None;
				this.Eb = false;
				this.ConstSpeed = false;
				this.HoldBrake = false;
				this.ReAdhesionDevice = ReadhesionDeviceType.TypeA;
				this.LoadCompensatingDevice = 0.0;
				this.PassAlarm = PassAlarmType.None;
				this.DoorOpenMode = DoorMode.AutomaticManualOverride;
				this.DoorCloseMode = DoorMode.AutomaticManualOverride;
				this.DoorWidth = 1000.0;
				this.DoorMaxTolerance = 0.0;
			}
		}
		
		// motor
		/// <summary>Any of the Motor sections of the train.dat. All members are stored in the unit as specified by the train.dat documentation.</summary>
		internal class Motor {
			
			internal BVEMotorSoundTableEntry[] Entries;
			internal Motor() {
				const int n = 800;
				this.Entries = new BVEMotorSoundTableEntry[n];
				for (int i = 0; i < n; i++) {
					this.Entries[i].SoundIndex = -1;
					this.Entries[i].Pitch = 100.0f;
					this.Entries[i].Gain = 128.0f;
				}
			}
		}
		
		// train
		/// <summary>The representation of the train.dat.</summary>
		internal class Train {
			internal Acceleration Acceleration;
			internal Performance Performance;
			internal Delay Delay;
			internal Move Move;
			internal Brake Brake;
			internal Pressure Pressure;
			internal Handle Handle;
			internal Cab Cab;
			internal Car Car;
			internal Device Device;
			internal Motor MotorP1;
			internal Motor MotorP2;
			internal Motor MotorB1;
			internal Motor MotorB2;
			internal Train () {
				this.Acceleration = new Acceleration();
				this.Performance = new Performance();
				this.Delay = new Delay();
				this.Move = new Move();
				this.Brake = new Brake();
				this.Pressure = new Pressure();
				this.Handle = new Handle();
				this.Cab = new Cab();
				this.Car = new Car();
				this.Device = new Device();
				this.MotorP1 = new Motor();
				this.MotorP2 = new Motor();
				this.MotorB1 = new Motor();
				this.MotorB2 = new Motor();
			}
		}

		const int currentVersion = 18230;

		// load
		/// <summary>Loads a file into an instance of the Train class.</summary>
		/// <param name="FileName">The train.dat file to load.</param>
		/// <returns>An instance of the Train class.</returns>
		internal static Train Load(string FileName) {
			Train t = new Train();
			t.Pressure.BrakePipeNormalPressure = 0.0;
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string[] Lines = System.IO.File.ReadAllLines(FileName, new System.Text.UTF8Encoding());
			for (int i = 0; i < Lines.Length; i++) {
				int j = Lines[i].IndexOf(';');
				if (j >= 0) {
					Lines[i] = Lines[i].Substring(0, j).Trim(new char[] { });
				} else {
					Lines[i] = Lines[i].Trim(new char[] { });
				}
			}
			bool ver1220000 = false;
			int v = 0;
			for (int i = 0; i < Lines.Length; i++) {
				if (Lines[i].Length != 0) {
					string s = Lines[i].ToLowerInvariant();
					switch (s)
					{
						case "bve1200000":
						case "bve1210000":
						case "bve1220000":
							ver1220000 = true;
							break;
						case "bve2000000":
						case "openbve":
							//No action
							break;
						default:
							if (s.ToLowerInvariant().StartsWith("openbve"))
							{
								string tt = s.Substring(7, s.Length - 7);
								if (int.TryParse(tt, System.Globalization.NumberStyles.Float, Culture, out v))
								{
									if (v > currentVersion)
									{
										MessageBox.Show("The train.dat " + FileName + " was created with a newer version of openBVE. Please check for an update.");
									}
								}
								else
								{
									MessageBox.Show("The train.dat version " + Lines[0].ToLowerInvariant() + " is invalid in " + FileName);
								}
							}
							break;
					}
					break;
				}
			}
			for (int i = 0; i < Lines.Length; i++) {
				int n = 0;
				switch (Lines[i].ToLowerInvariant()) {
					case "#acceleration":
						i++;
						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							if (n == t.Acceleration.Entries.Length) {
								Array.Resize(ref t.Acceleration.Entries, t.Acceleration.Entries.Length << 1);
								for (int o = n; o < t.Acceleration.Entries.Length; o++)
								{
									t.Acceleration.Entries[o] = new BveAccelerationCurve();
								}
							}
							string u = Lines[i] + ",";
							int m = 0;
							while (true) {
								int j = u.IndexOf(',');
								if (j == -1) break;
								string s = u.Substring(0, j).Trim(new char[] { });
								u = u.Substring(j + 1);
								double a; if (double.TryParse(s, System.Globalization.NumberStyles.Float, Culture, out a)) {
									switch (m) {
										case 0:
											t.Acceleration.Entries[n].StageZeroAcceleration = Math.Max(a, 0.0);
											break;
										case 1:
											t.Acceleration.Entries[n].StageOneAcceleration = Math.Max(a, 0.0);
											break;
										case 2:
											t.Acceleration.Entries[n].StageOneSpeed = Math.Max(a, 0.0);
											break;
										case 3:
											t.Acceleration.Entries[n].StageTwoSpeed = Math.Max(a, 0.0);
											if (t.Acceleration.Entries[n].StageTwoSpeed < t.Acceleration.Entries[n].StageOneSpeed) {
												double x = t.Acceleration.Entries[n].StageOneSpeed;
												t.Acceleration.Entries[n].StageOneSpeed = t.Acceleration.Entries[n].StageTwoSpeed;
												t.Acceleration.Entries[n].StageTwoSpeed = x;
											}
											break;
										case 4:
											if (ver1220000) {
												if (a <= 0.0) {
													t.Acceleration.Entries[n].StageTwoExponent = 1.0;
												} else {
													const double c = 1.23315173118822;
													t.Acceleration.Entries[n].StageTwoExponent = 1.0 - Math.Log(a) * t.Acceleration.Entries[n].StageTwoSpeed * c;
													if (t.Acceleration.Entries[n].StageTwoExponent > 4.0) {
														t.Acceleration.Entries[n].StageTwoExponent = 4.0;
													}
												}
											} else {
												t.Acceleration.Entries[n].StageTwoExponent = a;
											}
											break;
									}
								} m++;
							}
							i++;
							n++;
						}
						Array.Resize(ref t.Acceleration.Entries, n);
						i--;
						break;
					case "#performance":
					case "#deceleration":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								switch (n) {
									case 0:
										if (a >= 0.0) t.Performance.Deceleration = a;
										break;
									case 1:
										if (a >= 0.0) t.Performance.CoefficientOfStaticFriction = a;
										break;
									case 3:
										if (a >= 0.0) t.Performance.CoefficientOfRollingResistance = a;
										break;
									case 4:
										if (a >= 0.0) t.Performance.AerodynamicDragCoefficient = a;
										break;
								}
							} i++; n++;
						} i--; break;
					case "#delay":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a;
							if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								switch (n) {
									case 0:
										if(a >= 0.0) t.Delay.DelayPowerUp = new[] { a };
										break;
									case 1:
										if(a >= 0.0) t.Delay.DelayPowerDown = new[] { a };
										break;
									case 2:
										if(a >= 0.0) t.Delay.DelayBrakeUp = new[] { a };
										break;
									case 3:
										if(a >= 0.0) t.Delay.DelayBrakeDown = new[] { a };
										break;
									case 4:
										if (v < 18230)
										{
											if(a >= 0.0) t.Delay.DelayLocoBrakeUp = new[] { a };
										}
										else
										{
											if(a >= 0.0) t.Delay.ElectricBrakeDelayUp = a;
										}
										break;
									case 5:
										if (v < 18230)
										{
											if(a >= 0.0) t.Delay.DelayLocoBrakeDown = new[] { a };
										}
										else
										{
											if(a >= 0.0) t.Delay.ElectricBrakeDelayDown = a;
										}
										break;
									case 6:
										if (v >= 18230)
										{
											if(a >= 0.0) t.Delay.DelayLocoBrakeUp = new[] { a };
										}
										break;
									case 7:
										if (v >= 18230)
										{
											if(a >= 0.0) t.Delay.DelayLocoBrakeDown = new[] { a };
										}
										break;
								}
							}
							else if (Lines[i].IndexOf(',') != -1)
							{
								switch (n)
								{
									case 0:
										t.Delay.DelayPowerUp = Lines[i].Split(',').Select(x => Double.Parse(x, Culture)).ToArray();
										break;
									case 1:
										t.Delay.DelayPowerDown = Lines[i].Split(',').Select(x => Double.Parse(x, Culture)).ToArray();
										break;
									case 2:
										t.Delay.DelayBrakeUp = Lines[i].Split(',').Select(x => Double.Parse(x, Culture)).ToArray();
										break;
									case 3:
										t.Delay.DelayBrakeDown = Lines[i].Split(',').Select(x => Double.Parse(x, Culture)).ToArray();
										break;
									case 4:
										if (v < 18230)
										{
											t.Delay.ElectricBrakeDelayUp = Lines[i].Split(',').Select(x => Double.Parse(x, Culture)).ToArray()[0];	
										}
										else
										{
											t.Delay.DelayLocoBrakeUp = Lines[i].Split(',').Select(x => Double.Parse(x, Culture)).ToArray();	
										}
										break;
									case 5:
										if (v < 18230)
										{
											t.Delay.ElectricBrakeDelayDown = Lines[i].Split(',').Select(x => Double.Parse(x, Culture)).ToArray()[0];	
										}
										else
										{
											t.Delay.DelayLocoBrakeDown = Lines[i].Split(',').Select(x => Double.Parse(x, Culture)).ToArray();	
										}
										break;
									case 6:
										if (v >= 18230)
										{
											t.Delay.DelayLocoBrakeUp = Lines[i].Split(',').Select(x => Double.Parse(x, Culture)).ToArray();
										}
										break;
									case 7:
										if (v >= 18230)
										{
											t.Delay.DelayLocoBrakeDown = Lines[i].Split(',').Select(x => Double.Parse(x, Culture)).ToArray();
										}
										break;
								}
							}
							i++; n++;
						} i--; break;
					case "#move":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								switch (n) {
									case 0:
										if(a >= 0.0) t.Move.JerkPowerUp = a;
										break;
									case 1:
										if(a >= 0.0) t.Move.JerkPowerDown = a;
										break;
									case 2:
										if(a >= 0.0) t.Move.JerkBrakeUp = a;
										break;
									case 3:
										if(a >= 0.0) t.Move.JerkBrakeDown = a;
										break;
									case 4:
										if(a >= 0.0) t.Move.BrakeCylinderUp = a;
										break;
									case 5:
										if(a >= 0.0) t.Move.BrakeCylinderDown = a;
										break;
								}
							} i++; n++;
						} i--; break;
					case "#brake":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								int b = (int)Math.Round(a);
								switch (n) {
									case 0:
										if (b >= 0 & b <= 2) t.Brake.BrakeType = (BrakeSystemType)b;
										break;
									case 1:
										if (b >= 0 & b <= 2) t.Brake.BrakeControlSystem = (EletropneumaticBrakeType)b;
										break;
									case 2:
										if (a >= 0.0) t.Brake.BrakeControlSpeed = a;
										break;
									case 3:
										if (a >= 0 && a < 3) t.Brake.LocoBrakeType = (Brake.LocoBrakeTypes) b;
										break;
								}
							} i++; n++;
						} i--; break;
					case "#pressure":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								switch (n) {
									case 0:
										if (a > 0.0) t.Pressure.BrakeCylinderServiceMaximumPressure = a;
										break;
									case 1:
										if (a > 0.0) t.Pressure.BrakeCylinderEmergencyMaximumPressure = a;
										break;
									case 2:
										if (a > 0.0) t.Pressure.MainReservoirMinimumPressure = a;
										break;
									case 3:
										if (a > 0.0) t.Pressure.MainReservoirMaximumPressure = a;
										break;
									case 4:
										if (a > 0.0) t.Pressure.BrakePipeNormalPressure = a;
										break;
								}
							} i++; n++;
						} i--; break;
					case "#handle":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								int b = (int)Math.Round(a);
								switch (n) {
									case 0:
										if (b >= 0  && b <= 3) t.Handle.HandleType = (HandleType)b;
										break;
									case 1:
										if (b > 0) t.Handle.PowerNotches = b;
										break;
									case 2:
										if (b > 0) t.Handle.BrakeNotches = b;
										break;
									case 3:
										if (b >= 0) t.Handle.PowerNotchReduceSteps = b;
										break;
									case 4:
										if (a >= 0 && a < 4) t.Handle.HandleBehaviour = (EbHandleBehaviour) b;
										break;
									case 5:
										if (b > 0) t.Handle.LocoBrakeNotches = b;
										break;
									case 6:
										if (a <= 0 && a > 3) t.Handle.LocoBrake = (LocoBrakeType) b;
										break;
									case 7:
										if (b > 0) t.Handle.DriverPowerNotches = b;
										break;
									case 8:
										if (b > 0) t.Handle.DriverBrakeNotches = b;
										break;
								}
							} i++; n++;
						} i--; break;
					case "#cockpit":
					case "#cab":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								switch (n) {
									case 0:
										t.Cab.Driver.X = a;
										break;
									case 1:
										t.Cab.Driver.Y = a;
										break;
									case 2:
										t.Cab.Driver.Z = a;
										break;
									case 3:
										t.Cab.DriverCar = (int)Math.Round(a);
										break;
								}
							} i++; n++;
						} i--; break;
					case "#car":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								int b = (int)Math.Round(a);
								switch (n) {
									case 0:
										if (a > 0.0) t.Car.MotorCarMass = a;
										break;
									case 1:
										if (b >= 1) t.Car.NumberOfMotorCars = b;
										break;
									case 2:
										if (a > 0.0) t.Car.TrailerCarMass = a;
										break;
									case 3:
										if (b >= 0) t.Car.NumberOfTrailerCars = b;
										break;
									case 4:
										if (b > 0.0) t.Car.LengthOfACar = a;
										break;
									case 5:
										t.Car.FrontCarIsAMotorCar = a == 1.0;
										break;
									case 6:
										if (a > 0.0) t.Car.WidthOfACar = a;
										break;
									case 7:
										if (a > 0.0) t.Car.HeightOfACar = a;
										break;
									case 8:
										t.Car.CenterOfGravityHeight = a;
										break;
									case 9:
										if (a > 0.0) t.Car.ExposedFrontalArea = a;
										break;
									case 10:
										if (a > 0.0) t.Car.UnexposedFrontalArea = a;
										break;
								}
							} i++; n++;
						} i--; break;
					case "#device":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
							double a; if (double.TryParse(Lines[i], System.Globalization.NumberStyles.Float, Culture, out a)) {
								int b = (int)Math.Round(a);
								switch (n) {
									case 0:
										if (b >= -1 & b <= 1) t.Device.Ats = (AtsModes)b;
										break;
									case 1:
										if (b >= 0 & b <= 2) t.Device.Atc = (AtcModes)b;
										break;
									case 2:
										t.Device.Eb = a == 1.0;
										break;
									case 3:
										t.Device.ConstSpeed = a == 1.0;
										break;
									case 4:
										t.Device.HoldBrake = a == 1.0;
										break;
									case 5:
										if (b >= -1 & b <= 3) t.Device.ReAdhesionDevice = (ReadhesionDeviceType)b;
										break;
									case 6:
										t.Device.LoadCompensatingDevice = a;
										break;
									case 7:
										if (b >= 0 & b <= 2) t.Device.PassAlarm = (PassAlarmType)b;
										break;
									case 8:
										if (b >= 0 & b <= 2) t.Device.DoorOpenMode = (DoorMode)b;
										break;
									case 9:
										if (b >= 0 & b <= 2) t.Device.DoorCloseMode = (DoorMode)b;
										break;
									case 10:
										if (a >= 0.0) t.Device.DoorWidth = a;
										break;
									case 11:
										if (a >= 0.0) t.Device.DoorMaxTolerance = a;
										break;
								}
							} i++; n++;
						} i--; break;
					case "#motor_p1":
					case "#motor_p2":
					case "#motor_b1":
					case "#motor_b2":
						{
							string section = Lines[i].ToLowerInvariant();
							i++;
							Motor m = new Motor();
							while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.InvariantCultureIgnoreCase)) {
								if (n == m.Entries.Length) {
									Array.Resize(ref m.Entries, m.Entries.Length << 1);
								}
								string u = Lines[i] + ",";
								int k = 0;
								while (true) {
									int j = u.IndexOf(',');
									if (j == -1) break;
									string s = u.Substring(0, j).Trim(new char[] { });
									u = u.Substring(j + 1);
									double a; if (double.TryParse(s, System.Globalization.NumberStyles.Float, Culture, out a)) {
										int b = (int)Math.Round(a);
										switch (k) {
											case 0:
												m.Entries[n].SoundIndex = b >= 0 ? b : -1;
												break;
											case 1:
												m.Entries[n].Pitch = (float)Math.Max(a, 0.0);
												break;
											case 2:
												m.Entries[n].Gain = (float)Math.Max(a, 0.0);
												break;
										}
									} k++;
								}
								i++;
								n++;
							}
							Array.Resize(ref m.Entries, n);
							i--;
							switch (section) {
								case "#motor_p1":
									t.MotorP1 = m;
									break;
								case "#motor_p2":
									t.MotorP2 = m;
									break;
								case "#motor_b1":
									t.MotorB1 = m;
									break;
								case "#motor_b2":
									t.MotorB2 = m;
									break;
							}
						}
						break;
				}
			}

			if (t.Delay.DelayPowerUp.Length < t.Handle.PowerNotches)
			{
				int l = t.Delay.DelayPowerUp.Length;
				Array.Resize(ref t.Delay.DelayPowerUp, t.Handle.PowerNotches);
				for (int i = l + 1; i < t.Delay.DelayPowerUp.Length; i++)
				{
					t.Delay.DelayPowerUp[i] = 0;
				}
			}
			if (t.Delay.DelayPowerDown.Length < t.Handle.PowerNotches)
			{
				int l = t.Delay.DelayPowerDown.Length;
				Array.Resize(ref t.Delay.DelayPowerDown, t.Handle.PowerNotches);
				for (int i = l + 1; i < t.Delay.DelayPowerDown.Length; i++)
				{
					t.Delay.DelayPowerDown[i] = 0;
				}
			}
			if (t.Pressure.BrakePipeNormalPressure <= 0.0) {
				if (t.Brake.BrakeType == BrakeSystemType.AutomaticAirBrake) {
					t.Pressure.BrakePipeNormalPressure = t.Pressure.BrakeCylinderEmergencyMaximumPressure + 0.75 * (t.Pressure.MainReservoirMinimumPressure - t.Pressure.BrakeCylinderEmergencyMaximumPressure);
					if (t.Pressure.BrakePipeNormalPressure > t.Pressure.MainReservoirMinimumPressure) {
						t.Pressure.BrakePipeNormalPressure = t.Pressure.MainReservoirMinimumPressure;
					}
				} else {
					if (t.Pressure.BrakeCylinderEmergencyMaximumPressure < 480000.0 & t.Pressure.MainReservoirMinimumPressure > 500000.0) {
						t.Pressure.BrakePipeNormalPressure = 490000.0;
					} else {
						t.Pressure.BrakePipeNormalPressure = t.Pressure.BrakeCylinderEmergencyMaximumPressure + 0.75 * (t.Pressure.MainReservoirMinimumPressure - t.Pressure.BrakeCylinderEmergencyMaximumPressure);
					}
				}
			}
			if (t.Brake.BrakeType == BrakeSystemType.AutomaticAirBrake) {
				t.Device.HoldBrake = false;
			}
			if (t.Device.HoldBrake & t.Handle.BrakeNotches <= 0) {
				t.Handle.BrakeNotches = 1;
			}
			if (t.Cab.DriverCar < 0 | t.Cab.DriverCar >= t.Car.NumberOfMotorCars + t.Car.NumberOfTrailerCars) {
				t.Cab.DriverCar = 0;
			}
			return t;
		}
		
		// save
		/// <summary>Saves an instance of the Train class into a specified file.</summary>
		/// <param name="FileName">The train.dat file to save.</param>
		/// <param name="t">An instance of the Train class to save.</param>
		internal static void Save(string FileName, Train t) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Text.StringBuilder b = new System.Text.StringBuilder();
			b.AppendLine("OPENBVE" + currentVersion);
			b.AppendLine("#ACCELERATION");
			if (t.Acceleration.Entries.Length > t.Handle.PowerNotches) {
				Array.Resize(ref t.Acceleration.Entries, t.Handle.PowerNotches);
			}
			for (int i = 0; i < t.Acceleration.Entries.Length; i++) {
				b.Append(t.Acceleration.Entries[i].StageZeroAcceleration.ToString(Culture) + ",");
				b.Append(t.Acceleration.Entries[i].StageOneAcceleration.ToString(Culture) + ",");
				b.Append(t.Acceleration.Entries[i].StageOneSpeed.ToString(Culture) + ",");
				b.Append(t.Acceleration.Entries[i].StageTwoSpeed.ToString(Culture) + ",");
				b.AppendLine(t.Acceleration.Entries[i].StageTwoExponent.ToString(Culture));
			}
			int n = 15;
			b.AppendLine("#PERFORMANCE");
			b.AppendLine(t.Performance.Deceleration.ToString(Culture).PadRight(n, ' ') + "; Deceleration");
			b.AppendLine(t.Performance.CoefficientOfStaticFriction.ToString(Culture).PadRight(n, ' ') + "; CoefficientOfStaticFriction");
			b.AppendLine("0".PadRight(n, ' ') + "; Reserved (not used)");
			b.AppendLine(t.Performance.CoefficientOfRollingResistance.ToString(Culture).PadRight(n, ' ') + "; CoefficientOfRollingResistance");
			b.AppendLine(t.Performance.AerodynamicDragCoefficient.ToString(Culture).PadRight(n, ' ') + "; AerodynamicDragCoefficient");
			b.AppendLine("#DELAY");
			b.AppendLine(string.Join(",", t.Delay.DelayPowerUp.Select(d => d.ToString(Culture)).ToList()).PadRight(n, ' ') + "; DelayPowerUp");
			b.AppendLine(string.Join(",", t.Delay.DelayPowerDown.Select(d => d.ToString(Culture)).ToList()).PadRight(n, ' ') + "; DelayPowerDown");
			b.AppendLine(string.Join(",", t.Delay.DelayBrakeUp.Select(d => d.ToString(Culture)).ToList()).PadRight(n, ' ') + "; DelayBrakeUp");
			b.AppendLine(string.Join(",", t.Delay.DelayBrakeDown.Select(d => d.ToString(Culture)).ToList()).PadRight(n, ' ') + "; DelayBrakeDown");
			b.AppendLine(t.Delay.ElectricBrakeDelayUp.ToString(Culture).PadRight(n, ' ') + "; ElectricBrakeDelayUp");
			b.AppendLine(t.Delay.ElectricBrakeDelayDown.ToString(Culture).PadRight(n, ' ') + "; ElectricBrakeDelayDown");
			b.AppendLine(string.Join(",", t.Delay.DelayLocoBrakeUp.Select(d => d.ToString(Culture)).ToList()).PadRight(n, ' ') + "; DelayLocoBrakeUp (1.5.3.4+)");
			b.AppendLine(string.Join(",", t.Delay.DelayLocoBrakeDown.Select(d => d.ToString(Culture)).ToList()).PadRight(n, ' ') + "; DelayLocoBrakeDown (1.5.3.4+)");
			b.AppendLine("#MOVE");
			b.AppendLine(t.Move.JerkPowerUp.ToString(Culture).PadRight(n, ' ') + "; JerkPowerUp");
			b.AppendLine(t.Move.JerkPowerDown.ToString(Culture).PadRight(n, ' ') + "; JerkPowerDown");
			b.AppendLine(t.Move.JerkBrakeUp.ToString(Culture).PadRight(n, ' ') + "; JerkBrakeUp");
			b.AppendLine(t.Move.JerkBrakeDown.ToString(Culture).PadRight(n, ' ') + "; JerkBrakeDown");
			b.AppendLine(t.Move.BrakeCylinderUp.ToString(Culture).PadRight(n, ' ') + "; BrakeCylinderUp");
			b.AppendLine(t.Move.BrakeCylinderDown.ToString(Culture).PadRight(n, ' ') + "; BrakeCylinderDown");
			b.AppendLine("#BRAKE");
			b.AppendLine(((int)t.Brake.BrakeType).ToString(Culture).PadRight(n, ' ') + "; BrakeType");
			b.AppendLine(((int)t.Brake.BrakeControlSystem).ToString(Culture).PadRight(n, ' ') + "; BrakeControlSystem");
			b.AppendLine(t.Brake.BrakeControlSpeed.ToString(Culture).PadRight(n, ' ') + "; BrakeControlSpeed");
			b.AppendLine(((int)t.Brake.LocoBrakeType).ToString(Culture).PadRight(n, ' ') + "; LocoBrakeType (1.5.3.4+)");
			b.AppendLine("#PRESSURE");
			b.AppendLine(t.Pressure.BrakeCylinderServiceMaximumPressure.ToString(Culture).PadRight(n, ' ') + "; BrakeCylinderServiceMaximumPressure");
			b.AppendLine(t.Pressure.BrakeCylinderEmergencyMaximumPressure.ToString(Culture).PadRight(n, ' ') + "; BrakeCylinderEmergencyMaximumPressure");
			b.AppendLine(t.Pressure.MainReservoirMinimumPressure.ToString(Culture).PadRight(n, ' ') + "; MainReservoirMinimumPressure");
			b.AppendLine(t.Pressure.MainReservoirMaximumPressure.ToString(Culture).PadRight(n, ' ') + "; MainReservoirMaximumPressure");
			b.AppendLine(t.Pressure.BrakePipeNormalPressure.ToString(Culture).PadRight(n, ' ') + "; BrakePipeNormalPressure");
			b.AppendLine("#HANDLE");
			b.AppendLine(((int)t.Handle.HandleType).ToString(Culture).PadRight(n, ' ') + "; HandleType");
			b.AppendLine(t.Handle.PowerNotches.ToString(Culture).PadRight(n, ' ') + "; PowerNotches");
			b.AppendLine(t.Handle.BrakeNotches.ToString(Culture).PadRight(n, ' ') + "; BrakeNotches");
			b.AppendLine(t.Handle.PowerNotchReduceSteps.ToString(Culture).PadRight(n, ' ') + "; PowerNotchReduceSteps");
			b.AppendLine(((int)t.Handle.HandleBehaviour).ToString(Culture).PadRight(n, ' ') + "; EbHandleBehaviour (1.5.3.3+)");
			b.AppendLine(t.Handle.LocoBrakeNotches.ToString(Culture).PadRight(n, ' ') + "; LocoBrakeNotches (1.5.3.4+)");
			b.AppendLine(((int)t.Handle.LocoBrake).ToString(Culture).PadRight(n, ' ') + "; LocoBrakeType (1.5.3.4+)");
			b.AppendLine(t.Handle.DriverPowerNotches.ToString(Culture).PadRight(n, ' ') + "; DriverPowerNotches");
			b.AppendLine(t.Handle.DriverBrakeNotches.ToString(Culture).PadRight(n, ' ') + "; DriverBrakeNotches");
			b.AppendLine("#CAB");
			b.AppendLine(t.Cab.Driver.X.ToString(Culture).PadRight(n, ' ') + "; X");
			b.AppendLine(t.Cab.Driver.Y.ToString(Culture).PadRight(n, ' ') + "; Y");
			b.AppendLine(t.Cab.Driver.Z.ToString(Culture).PadRight(n, ' ') + "; Z");
			b.AppendLine(t.Cab.DriverCar.ToString(Culture).PadRight(n, ' ') + "; DriverCar");
			b.AppendLine("#CAR");
			b.AppendLine(t.Car.MotorCarMass.ToString(Culture).PadRight(n, ' ') + "; MotorCarMass");
			b.AppendLine(t.Car.NumberOfMotorCars.ToString(Culture).PadRight(n, ' ') + "; NumberOfMotorCars");
			b.AppendLine(t.Car.TrailerCarMass.ToString(Culture).PadRight(n, ' ') + "; TrailerCarMass");
			b.AppendLine(t.Car.NumberOfTrailerCars.ToString(Culture).PadRight(n, ' ') + "; NumberOfTrailerCars");
			b.AppendLine(t.Car.LengthOfACar.ToString(Culture).PadRight(n, ' ') + "; LengthOfACar");
			b.AppendLine((t.Car.FrontCarIsAMotorCar ? "1" : "0").PadRight(n, ' ') + "; FrontCarIsAMotorCar");
			b.AppendLine(t.Car.WidthOfACar.ToString(Culture).PadRight(n, ' ') + "; WidthOfACar");
			b.AppendLine(t.Car.HeightOfACar.ToString(Culture).PadRight(n, ' ') + "; HeightOfACar");
			b.AppendLine(t.Car.CenterOfGravityHeight.ToString(Culture).PadRight(n, ' ') + "; CenterOfGravityHeight");
			b.AppendLine(t.Car.ExposedFrontalArea.ToString(Culture).PadRight(n, ' ') + "; ExposedFrontalArea");
			b.AppendLine(t.Car.UnexposedFrontalArea.ToString(Culture).PadRight(n, ' ') + "; UnexposedFrontalArea");
			b.AppendLine("#DEVICE");
			b.AppendLine(((int)t.Device.Ats).ToString(Culture).PadRight(n, ' ') + "; Ats");
			b.AppendLine(((int)t.Device.Atc).ToString(Culture).PadRight(n, ' ') + "; Atc");
			b.AppendLine((t.Device.Eb ? "1" : "0").PadRight(n, ' ') + "; Eb");
			b.AppendLine((t.Device.ConstSpeed ? "1" : "0").PadRight(n, ' ') + "; ConstSpeed");
			b.AppendLine((t.Device.HoldBrake ? "1" : "0").PadRight(n, ' ') + "; HoldBrake");
			b.AppendLine(((int)t.Device.ReAdhesionDevice).ToString(Culture).PadRight(n, ' ') + "; ReAdhesionDevice");
			b.AppendLine(t.Device.LoadCompensatingDevice.ToString(Culture).PadRight(n, ' ') + "; Reserved (not used)");
			b.AppendLine(((int)t.Device.PassAlarm).ToString(Culture).PadRight(n, ' ') + "; PassAlarm");
			b.AppendLine(((int)t.Device.DoorOpenMode).ToString(Culture).PadRight(n, ' ') + "; DoorOpenMode");
			b.AppendLine(((int)t.Device.DoorCloseMode).ToString(Culture).PadRight(n, ' ') + "; DoorCloseMode");
			b.AppendLine(t.Device.DoorWidth.ToString(Culture).PadRight(n, ' ') + "; DoorWidth");
			b.AppendLine(t.Device.DoorMaxTolerance.ToString(Culture).PadRight(n, ' ') + "; DoorMaxTolerance");
			for (int i = 0; i < 4; i++) {
				Motor m = null;
				switch (i) {
					case 0:
						b.AppendLine("#MOTOR_P1");
						m = t.MotorP1;
						break;
					case 1:
						b.AppendLine("#MOTOR_P2");
						m = t.MotorP2;
						break;
					case 2:
						b.AppendLine("#MOTOR_B1");
						m = t.MotorB1;
						break;
					case 3:
						b.AppendLine("#MOTOR_B2");
						m = t.MotorB2;
						break;
				}
				int k;
				for (k = m.Entries.Length - 1; k >= 0; k--) {
					if (m.Entries[k].SoundIndex >= 0) break;
				}
				k = Math.Min(k + 2, m.Entries.Length);
				Array.Resize(ref m.Entries, k);
				for (int j = 0; j < m.Entries.Length; j++) {
					b.Append(m.Entries[j].SoundIndex.ToString(Culture) + ",");
					b.Append(m.Entries[j].Pitch.ToString(Culture) + ",");
					b.AppendLine(m.Entries[j].Gain.ToString(Culture));
				}
			}
			System.IO.File.WriteAllText(FileName, b.ToString(), new System.Text.UTF8Encoding(true));
		}

	}
}
