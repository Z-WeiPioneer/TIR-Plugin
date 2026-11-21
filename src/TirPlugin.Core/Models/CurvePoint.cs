namespace TirPlugin.Core.Models
{
    /// <summary>
    /// 目标配光曲线上的一个控制点（极坐标表示：半径方向 + 角度）。
    /// 双模输入（快捷参数 / 曲线拖拽）共享同一组控制点，避免"文本一套、曲线一套"。
    /// </summary>
    public sealed class CurvePoint
    {
        /// <summary>创建一个控制点（R = 0，ThetaDeg = 0）。</summary>
        public CurvePoint()
        {
            R = 0.0;
            ThetaDeg = 0.0;
        }

        /// <summary>创建一个控制点。</summary>
        /// <param name="r">极坐标半径（相对光强或归一化光强，具体口径由曲线模型约定）。</param>
        /// <param name="thetaDeg">角度，单位：度。</param>
        public CurvePoint(double r, double thetaDeg)
        {
            R = r;
            ThetaDeg = thetaDeg;
        }

        /// <summary>极坐标半径（相对光强，通常 0–1）。</summary>
        public double R { get; set; }

        /// <summary>角度，单位：度（通常 0–90）。</summary>
        public double ThetaDeg { get; set; }

        /// <summary>返回便于日志/测试输出的简短文本。</summary>
        public override string ToString()
        {
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "CurvePoint(R={0:0.####}, ThetaDeg={1:0.####})",
                R,
                ThetaDeg);
        }
    }
}
