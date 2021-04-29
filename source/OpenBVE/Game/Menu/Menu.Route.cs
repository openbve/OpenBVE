﻿using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using LibRender2;
using LibRender2.Primitives;
using OpenBveApi;
using OpenBveApi.Textures;
using RouteManager2;
using Path = OpenBveApi.Path;

namespace OpenBve
{
	public partial class Menu
	{
		private static BackgroundWorker routeWorkerThread;
		private static string RouteSearchDirectory;
		private static string RouteFile;
		private static string TrainFolder;
		private static Encoding RouteEncoding;
		private static RouteState RoutefileState;
		private static Texture routeTexture;
		private static readonly Textbox routeDescriptionBox = new Textbox(Program.Renderer, Program.Renderer.Fonts.NormalFont);

		private static void routeWorkerThread_doWork(object sender, DoWorkEventArgs e)
		{
			if (string.IsNullOrEmpty(RouteFile))
			{
				return;
			}
			RouteEncoding = TextEncoding.GetSystemEncodingFromFile(RouteFile);
			Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\loading.png"), new TextureParameters(null, null), out routeTexture);
			Game.Reset(false);
			bool loaded = false;
			for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
			{
				if (Program.CurrentHost.Plugins[i].Route != null && Program.CurrentHost.Plugins[i].Route.CanLoadRoute(RouteFile))
				{
					object Route = (object)Program.CurrentRoute; //must cast to allow us to use the ref keyword.
					string RailwayFolder = Loading.GetRailwayFolder(RouteFile);
					string ObjectFolder = OpenBveApi.Path.CombineDirectory(RailwayFolder, "Object");
					string SoundFolder = OpenBveApi.Path.CombineDirectory(RailwayFolder, "Sound");
					if (Program.CurrentHost.Plugins[i].Route.LoadRoute(RouteFile, RouteEncoding, null, ObjectFolder, SoundFolder, true, ref Route))
					{
						Program.CurrentRoute = (CurrentRoute) Route;
					}
					else
					{
						if (Program.CurrentHost.Plugins[i].Route.LastException != null)
						{
							throw Program.CurrentHost.Plugins[i].Route.LastException; //Re-throw last exception generated by the route parser plugin so that the UI thread captures it
						}
						routeDescriptionBox.Text = "An unknown error was enountered whilst attempting to parse the routefile " + RouteFile;
						RoutefileState = RouteState.Error;
					}
					loaded = true;
					break;
				}
			}

			if (!loaded)
			{
				throw new Exception("No plugins capable of loading routefile " + RouteFile + " were found.");
			}
		}

		private static void routeWorkerThread_completed(object sender, RunWorkerCompletedEventArgs e)
		{
			RoutefileState = RouteState.Processed;
			if (e.Error != null || Program.CurrentRoute == null)
			{
				Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\route_error.png"), new TextureParameters(null, null), out routeTexture);
				if (e.Error != null)
				{
					routeDescriptionBox.Text = e.Error.Message;
					RoutefileState = RouteState.Error;
				}
				//pictureboxRouteMap.Image = null;
				//pictureboxRouteGradient.Image = null;
				//Result.ErrorFile = Result.RouteFile;
				//RouteFile = string.Empty;
				//checkboxTrainDefault.Text = Translations.GetInterfaceString("start_train_usedefault");
				routeWorkerThread.Dispose();
				return;
			}
			try
			{
				lock (BaseRenderer.GdiPlusLock)
				{
					//pictureboxRouteMap.Image = Illustrations.CreateRouteMap(pictureboxRouteMap.Width, pictureboxRouteMap.Height, false);
					//pictureboxRouteGradient.Image = Illustrations.CreateRouteGradientProfile(pictureboxRouteGradient.Width,
					//	pictureboxRouteGradient.Height, false);
				}
				// image
				if (!string.IsNullOrEmpty(Program.CurrentRoute.Image))
				{

					try
					{
						if (File.Exists(Program.CurrentRoute.Image))
						{
							Program.CurrentHost.RegisterTexture(Program.CurrentRoute.Image, new TextureParameters(null, null), out routeTexture);
						}
						else
						{
							Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\route_unknown.png"), new TextureParameters(null, null), out routeTexture);
						}
						
					}
					catch
					{
						routeTexture = null;
					}
				}
				else
				{
					string[] f = {".png", ".bmp", ".gif", ".tiff", ".tif", ".jpeg", ".jpg"};
					int i;
					for (i = 0; i < f.Length; i++)
					{
						string g = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(RouteFile),
							System.IO.Path.GetFileNameWithoutExtension(RouteFile) + f[i]);
						if (System.IO.File.Exists(g))
						{
							try
							{
								using (var fs = new FileStream(g, FileMode.Open, FileAccess.Read))
								{
									//pictureboxRouteImage.Image = new Bitmap(fs);
								}
							}
							catch
							{
								//pictureboxRouteImage.Image = null;
							}
							break;
						}
					}
					if (i == f.Length)
					{
						Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\route_unknown.png"), new TextureParameters(null, null), out routeTexture);
					}
				}

				// description
				string Description = Program.CurrentRoute.Comment.ConvertNewlinesToCrLf();
				if (Description.Length != 0)
				{
					routeDescriptionBox.Text = Description;
				}
				else
				{
					routeDescriptionBox.Text = System.IO.Path.GetFileNameWithoutExtension(RouteFile);
				}

				//textboxRouteEncodingPreview.Text = Description.ConvertNewlinesToCrLf();
				if (Interface.CurrentOptions.TrainName != null)
				{
				//	checkboxTrainDefault.Text = Translations.GetInterfaceString("start_train_usedefault") + @" (" + Interface.CurrentOptions.TrainName + @")";
				}
				else
				{
				//	checkboxTrainDefault.Text = Translations.GetInterfaceString("start_train_usedefault");
				}
				//Result.ErrorFile = null;
			}
			catch (Exception ex)
			{
				Program.CurrentHost.RegisterTexture(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\route_error.png"), new TextureParameters(null, null), out routeTexture);
				routeDescriptionBox.Text = ex.Message;
				//textboxRouteEncodingPreview.Text = "";
				//pictureboxRouteMap.Image = null;
				//pictureboxRouteGradient.Image = null;
				//Result.ErrorFile = Result.RouteFile;
				RouteFile = null;
				//checkboxTrainDefault.Text = Translations.GetInterfaceString("start_train_usedefault");
			}
		}
	}
}
