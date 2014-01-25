using System;
using DotSpatial.Projections;
using System.Drawing;
using System.Collections.Generic;

namespace WorldFileResolver
{
	//A〜Eの定義はこちら参照 : http://en.wikipedia.org/wiki/World_file
	public class MapPoint {
		public PointD wgs84Point;
		public PointD projPoint;
		public PointD pixelPoint;

		public PointD projVector;
		public PointD pixelVector;

		public WorldParameter worldParameter;
	}

	public struct WorldParameter {
		public double A;
		public double B;
		public double C;
		public double D;
		public double E;
		public double F;
	}

	public class Resolver
	{
		ProjectionInfo pWGS84 = KnownCoordinateSystems.Geographic.World.WGS1984;
		ProjectionInfo pMine = null;
		List<MapPoint> MapPoints = new List<MapPoint> ();
		MapPoint Center = new MapPoint {
			projPoint   = new PointD { X = 0.0, Y = 0.0 },
			projVector  = new PointD { X = 0.0, Y = 0.0 },
			pixelPoint  = new PointD { X = 0.0, Y = 0.0 },
			pixelVector = new PointD { X = 0.0, Y = 0.0 },
			worldParameter = new WorldParameter {
				A = 0.0, B = 0.0, C = 0.0, D = 0.0, E = 0.0, F = 0.0
			}
		};

		//proj無指定の場合はwgs84=proj座標として扱う（というか座標不問となる）
		//経緯度ではなくいきなり投影系座標を元座標として使う場合はこちら
		public Resolver ()
		{}

		//投影系をただ一つ入力した場合は、入力された経緯度を投影系座標に直し、
		//その投影系座標とピクセル座標の間で、ワールドファイルパラメータを推定する。
		public Resolver (string projText)
		{
			pMine = ProjectionInfo.FromProj4String (projText); 
		}

		public Resolver (int epsgCode)
		{
			pMine = ProjectionInfo.FromEpsgCode (epsgCode);
		}

		//2つ投影系を入力した場合は、入力経緯度を1つ目の座標系として想定し、2つ目の座標系に変換後、
		//ワールドファイルパラメータを推定する。
		//経緯度が旧日本測地系等の場合を想定。
		public Resolver (string fromText, string toText) : this(toText)
		{
			pWGS84 = ProjectionInfo.FromProj4String (fromText); 
		}

		public Resolver (string fromText, int toCode) : this(toCode)
		{
			pWGS84 = ProjectionInfo.FromProj4String (fromText); 
		}

		public Resolver (int fromCode, string toText) : this(toText)
		{
			pWGS84 = ProjectionInfo.FromEpsgCode (fromCode);
		}

		public Resolver (int fromCode, int toCode) : this(toCode)
		{
			pWGS84 = ProjectionInfo.FromEpsgCode (fromCode);
		}

		public void AddMapPoint(double longitude, double latitude, double pixelX, double pixelY)
		{
			var newPoint = new MapPoint { 
				wgs84Point = new PointD { X = longitude, Y = latitude },
				pixelPoint = new PointD { X = pixelX,    Y = pixelY }
			};
			if (pMine == null) {
				newPoint.projPoint = newPoint.wgs84Point;
			} else {
				var xy = new double[2]{ longitude, latitude };
				var z = new double[1]{ 0.0 };
				Reproject.ReprojectPoints (xy, z, pWGS84, pMine, 0, 1);
				newPoint.projPoint = new PointD { X = xy [0], Y = xy [1] };
			}
			MapPoints.Add (newPoint);
		}

		public WorldParameter Resolve() {
			var count = this.MapPoints.Count;

			//まず投影座標,ピクセル座標の重心を求める
			foreach (var point in this.MapPoints) {
				Center.projPoint.X  += point.projPoint.X;
				Center.projPoint.Y  += point.projPoint.Y;
				Center.pixelPoint.X += point.pixelPoint.X;
				Center.pixelPoint.Y += point.pixelPoint.Y;
			}
			Center.projPoint.X  /= count;
			Center.projPoint.Y  /= count;
			Center.pixelPoint.X /= count;
			Center.pixelPoint.Y /= count;

			//投影座標、ピクセル座標双方の重心ベクタを求める、その値からA,Eを求める
			foreach (var point in this.MapPoints) {
				point.projVector = new PointD {
					X = point.projPoint.X - Center.projPoint.X,
					Y = point.projPoint.Y - Center.projPoint.Y
				};
				point.pixelVector = new PointD { 
					X = point.pixelPoint.X - Center.pixelPoint.X,
					Y = point.pixelPoint.Y - Center.pixelPoint.Y
				};
				point.worldParameter.A = point.projVector.X / point.pixelVector.X;
				point.worldParameter.E = point.projVector.Y / point.pixelVector.Y;
				Center.worldParameter.A += point.worldParameter.A;
				Center.worldParameter.E += point.worldParameter.E;
			}
			Center.worldParameter.A /= count;
			Center.worldParameter.E /= count;

			//B,Dの算出
			foreach (var point in this.MapPoints) {
				point.worldParameter.B =  (point.projVector.X - point.pixelVector.X * Center.worldParameter.A) / point.pixelVector.Y;
				point.worldParameter.D =  (point.projVector.Y - point.pixelVector.Y * Center.worldParameter.E) / point.pixelVector.X;

				Center.worldParameter.B += point.worldParameter.B;
				Center.worldParameter.D += point.worldParameter.D;
			}
			Center.worldParameter.B /= count;
			Center.worldParameter.D /= count;

			//C,Fの算出
			foreach (var point in this.MapPoints) {
				point.worldParameter.C = point.projPoint.X - (point.pixelPoint.X * Center.worldParameter.A + point.pixelPoint.Y * Center.worldParameter.B);
				point.worldParameter.F = point.projPoint.Y - (point.pixelPoint.Y * Center.worldParameter.E + point.pixelPoint.X * Center.worldParameter.D);

				Center.worldParameter.C += point.worldParameter.C;
				Center.worldParameter.F += point.worldParameter.F;
			}
			Center.worldParameter.C /= count;
			Center.worldParameter.F /= count;

			return Center.worldParameter;
		}
	}
}

