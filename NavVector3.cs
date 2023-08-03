/*************************************************
	作者: Plane
	邮箱: 1785275942@qq.com	
	功能: 导航计算专用向量

    //=================*=================\\
           教学官网：www.qiqiker.com
           官方微信服务号: qiqikertuts
           Plane老师微信: PlaneZhong
               ~~获取更多教学资讯~~
    \\=================*=================//
*************************************************/

using System;
#if UnityView
using UnityEngine;
#endif

namespace PENav {
    public struct NavVector3 {
        public float x;
        public float y;
        public float z;

        public NavVector3(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public NavVector3(float x, float z) {
            this.x = x;
            this.y = 0;
            this.z = z;
        }
#if UnityView
        public NavVector3(Vector3 v) {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }
#endif
        public static NavVector3 Zero {
            get {
                return new NavVector3(0, 0, 0);
            }
        }
        public static NavVector3 One {
            get {
                return new NavVector3(1, 1, 1);
            }
        }
        public static NavVector3 operator +(NavVector3 v1, NavVector3 v2) {
            return new NavVector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }
        public static NavVector3 operator -(NavVector3 v1, NavVector3 v2) {
            return new NavVector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }
        public static NavVector3 operator *(NavVector3 v, float value) {
            return new NavVector3(v.x * value, v.y * value, v.z * value);
        }
        public static NavVector3 operator /(NavVector3 v, float value) {
            return new NavVector3(v.x / value, v.y / value, v.z / value);
        }
        public static bool operator ==(NavVector3 v1, NavVector3 v2) {
            return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
        }
        public static bool operator !=(NavVector3 v1, NavVector3 v2) {
            return v1.x != v2.x || v1.y != v2.y || v1.z != v2.z;
        }

        /// <summary>
        /// 二维向量点乘
        /// 反应的是V1向量在V2向量上的投影与V2向量模的乘积
        ///
        /// 值大于0，方向基本相同，夹角在0~90°之间
        /// 值等于0，相互垂直
        /// 值小于0，方向基本相反，夹角在90~180°之间
        /// </summary>
        public static float DotXZ(NavVector3 v1, NavVector3 v2) {
            return v1.x * v2.x + v1.z * v2.z;
        }
        /// 三维向量叉乘
        public static NavVector3 CrossXYZ(NavVector3 v1, NavVector3 v2) {
            return new NavVector3() {
                x = v1.y * v2.z - v1.z * v2.y,
                y = v1.z * v2.x - v1.x * v2.z,
                z = v1.x * v2.y - v1.y * v2.x
            };
        }
        
        /// <summary>
        /// 二维向量叉乘
        /// </summary>
        /// 值为负：V2在V1顺时针方向
        /// 值为正：V2在V1逆时针方向
        /// 值为0：V2和V1共线 (方向相同或相反)
        /// (x1,y1) x (x2,y2) = x1*y2 - x2*y1
        public static float CrossXZ(NavVector3 v1, NavVector3 v2) {
            return v1.x * v2.z - v2.x * v1.z;
        }
        
        //规格化
        public static NavVector3 NormalXZ(NavVector3 v) {
            float len = MathF.Sqrt(v.x * v.x + v.z * v.z);
            NavVector3 nor = new NavVector3 {
                x = v.x / len,
                y = 0,
                z = v.z / len
            };
            return nor;
        }
        
        //传入的向量都是单位向量
        public static float AngleXZ(NavVector3 v1, NavVector3 v2) {
            float dot = DotXZ(v1, v2);
            //Unity中 顺时针为正，逆时针为负
            float angle = MathF.Acos(dot); //弧度值
            
            //考虑角度的正负 这么返回的值就是一个带有方向的角度值，一般其实不需要
            NavVector3 cross = CrossXYZ(v1, v2);
            if(cross.y < 0) {
                angle = -angle;
            }
            return angle;
        }
        public static float SqrDis(NavVector3 v1, NavVector3 v2) {
            return (v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y) + (v1.z - v2.z) * (v1.z - v2.z);
        }

        public override bool Equals(object obj) {
            return obj is NavVector3 vector &&
                   x == vector.x &&
                   y == vector.y &&
                   z == vector.z;
        }
        public override int GetHashCode() {
            return HashCode.Combine(x, y, z);
        }
#if  UnityView
        public Vector3 ConvertUnityVector() {
            return new Vector3(x, y, z);
        }
#endif
        public override string ToString() {
            return $"[{x},{z}]";
        }
    }
}