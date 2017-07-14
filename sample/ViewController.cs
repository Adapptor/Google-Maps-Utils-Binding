﻿﻿﻿﻿using System;

using UIKit;
using Adapptor.GoogleMapsUtils.Ios;
using Google.Maps;
using CoreGraphics;
using Foundation;

namespace iOSMapUtil
{
	public partial class ViewController : UIViewController, IGMUClusterRendererDelegate, IGMUClusterManagerDelegate, IMapViewDelegate
	{
		const int kClusterItemCount = 10000;
        // const double kCameraLatitude = -33.8;
        // const double kCameraLongitude = 151.2;
        //const double kCameraLatitude = 37.4220;
        //const double kCameraLongitude = -122.0841;
        const double kCameraLatitude = 37.67395167941667;
        const double kCameraLongitude = 15.02468937557116;

		const double extent = 0.2;

		MapView mapView;
		GMUClusterManager clusterManager;

		protected ViewController(IntPtr handle) : base(handle)
		{
		}

		public override void LoadView ()
		{
			base.LoadView ();

			var camera = CameraPosition.FromCamera (kCameraLatitude, kCameraLongitude, 17);
			mapView = MapView.FromCamera (CGRect.Empty, camera);
			mapView.MyLocationEnabled = true;

			View = mapView;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// AddCluster ();
            AddKml();
		}

		public double GetRandomNumber(double minimum, double maximum)
		{
			Random random = new Random();
			return random.NextDouble() * (maximum - minimum) + minimum;
		}

		void AddCluster ()
		{
			var googleMapView = mapView; //use the real mapview init'd somewhere else instead of this
			var iconGenerator = new GMUDefaultClusterIconGenerator ();
			var algorithm = new GMUNonHierarchicalDistanceBasedAlgorithm ();
			var renderer = new GMUDefaultClusterRenderer (googleMapView, iconGenerator);

			renderer.WeakDelegate = this;

			clusterManager = new GMUClusterManager (googleMapView, algorithm, renderer);

			for (var i = 0; i <= kClusterItemCount; i++) {
				var lat = kCameraLatitude + extent * GetRandomNumber (-1.0, 1.0);

				var lng = kCameraLongitude + extent * GetRandomNumber (-1.0, 1.0);

				var name = $"Item {i}";

				IGMUClusterItem item = new POIItem (lat, lng, name);

				clusterManager.AddItem (item);
			}

			clusterManager.Cluster ();

			clusterManager.SetDelegate (this, this);
		}

        void AddKml()
        {
            var path = NSBundle.MainBundle.PathForResource("KML_Samples", "kml");
            // var url = new NSUrl(path);
            var data = NSData.FromFile(path);
            var kmlParser = new GMUKMLParser(data);
            kmlParser.Parse();

            // var renderer = new GMUGeometryRenderer(mapView, kmlParser.Placemarks, kmlParser.Styles);
            var placemarks = kmlParser.Placemarks;
            var styles = kmlParser.Styles;
            var renderer = new GMUGeometryRenderer(mapView, placemarks);
            renderer.Render();
        }

		[Export ("renderer:willRenderMarker:")]
		public void WillRenderMarker (GMUClusterRenderer renderer, Overlay marker)
		{
			if (marker is Marker) { // Overlays sneaking in here disguised as Markers...
				var myMarker = (Marker)marker;

				if (myMarker.UserData is POIItem) {
					POIItem item = (POIItem)myMarker.UserData;
					myMarker.Title = item.Name;
				}
			}
		}

		[Export ("clusterManager:didTapCluster:")]
		public void DidTapCluster (GMUClusterManager clusterManager, IGMUCluster cluster)
		{
			var newCamera = CameraPosition.FromCamera (cluster.Position, mapView.Camera.Zoom + 1);

			var update = CameraUpdate.SetCamera (newCamera);

			mapView.MoveCamera (update);
		}
	}
}
