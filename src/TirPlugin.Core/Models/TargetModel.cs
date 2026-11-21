using System.Collections.Generic;
using System.Globalization;

namespace TirPlugin.Core.Models
{
    /// <summary>
    /// 光斑目标类型（决定目标配光的评价空间）。
    /// </summary>
    public enum SpotType
    {
        /// <summary>远场角域：评价光强-角度分布，不需要接收面距离。</summary>
        FarField = 0,

        /// <summary>面域（指定距离的照度场）：必须填写 <see cref="TargetModel.DistanceM"/>。</summary>
        NearField = 1
    }

    /// <summary>
    /// 设计目标模型 —— 双模输入（快捷参数 / 配光曲线拖拽）的唯一数据源。
    /// 字段口径见《TIR 插件功能与接口规格 V1.1》输入参数表与数据契约草案。
    /// </summary>
    public sealed class TargetModel
    {
        /// <summary>控制点数量下限（列表非空时）。</summary>
        public const int MinControlPoints = 5;

        /// <summary>控制点数量上限（列表非空时）。</summary>
        public const int MaxControlPoints = 32;

        /// <summary>光束角默认值（2θ½ / FWHM），单位：度。</summary>
        public const double DefaultBeamAngleDeg = 30.0;

        /// <summary>均匀性默认值（0–1）。</summary>
        public const double DefaultUniformity = 0.80;

        /// <summary>效率默认值（0–1）。</summary>
        public const double DefaultEfficiency = 0.85;

        /// <summary>默认随机种子（固定种子保证 LT/优化结果可复现）。</summary>
        public const int DefaultSeed = 20261121;

        /// <summary>按《功能与接口规格 V1.1》默认值初始化目标模型。</summary>
        public TargetModel()
        {
            BeamAngleDeg = DefaultBeamAngleDeg;
            SpotType = SpotType.FarField;
            DistanceM = null;
            Uniformity = DefaultUniformity;
            Efficiency = DefaultEfficiency;
            SourceWidthMm = 2.0;
            SourceHeightMm = 2.0;
            MaterialName = "PMMA";
            RefractiveIndex = 1.49;
            LensDiameterMm = 34.0;
            LensHeightMm = 18.0;
            ControlPoints = new List<CurvePoint>();
            Seed = DefaultSeed;
        }

        /// <summary>光束角 2θ½（FWHM），单位：度；允许范围 5–90，默认 30。</summary>
        public double BeamAngleDeg { get; set; }

        /// <summary>光斑目标类型（远场角域 / 指定距离面域），默认远场。</summary>
        public SpotType SpotType { get; set; }

        /// <summary>接收面距离，单位：米；面域（NearField）类型时必填，远场可为 null。</summary>
        public double? DistanceM { get; set; }

        /// <summary>目标均匀性 U（0–1），默认 0.80。</summary>
        public double Uniformity { get; set; }

        /// <summary>目标效率 η（0–1），默认 0.85。</summary>
        public double Efficiency { get; set; }

        /// <summary>LED 发光面宽度，单位：毫米，默认 2 mm。</summary>
        public double SourceWidthMm { get; set; }

        /// <summary>LED 发光面高度，单位：毫米，默认 2 mm。</summary>
        public double SourceHeightMm { get; set; }

        /// <summary>透镜材料牌号，默认 PMMA。</summary>
        public string MaterialName { get; set; }

        /// <summary>材料折射率 n_d，默认 1.49（PMMA）；PC 约 1.58。</summary>
        public double RefractiveIndex { get; set; }

        /// <summary>透镜口径（外径）D，单位：毫米，默认 34 mm。</summary>
        public double LensDiameterMm { get; set; }

        /// <summary>透镜总高 H，单位：毫米，默认 18 mm。</summary>
        public double LensHeightMm { get; set; }

        /// <summary>目标配光曲线控制点；为空表示稍后由算法/导入生成。</summary>
        public List<CurvePoint> ControlPoints { get; set; }

        /// <summary>随机种子，默认 20261121；固定种子保证结果可复现。</summary>
        public int Seed { get; set; }

        /// <summary>
        /// 校验目标模型是否满足求解前置条件（不做求解、不访问任何外部程序）。
        /// </summary>
        /// <param name="error">校验失败时的中文错误消息；校验通过时为 null。</param>
        /// <returns>全部规则通过返回 true，否则返回 false。</returns>
        public bool Validate(out string error)
        {
            if (SpotType != SpotType.FarField && SpotType != SpotType.NearField)
            {
                error = string.Format(
                    CultureInfo.InvariantCulture,
                    "未知的光斑类型 SpotType：{0}。",
                    (int)SpotType);
                return false;
            }

            // 注意：使用取反写法以便 NaN 也判为非法（NaN 与任何数比较均为 false）。
            if (!(BeamAngleDeg >= 5.0 && BeamAngleDeg <= 90.0))
            {
                error = string.Format(
                    CultureInfo.InvariantCulture,
                    "光束角 2θ½ 必须在 5°–90° 之间（当前 {0:0.####}°）。",
                    BeamAngleDeg);
                return false;
            }

            if (!(Uniformity >= 0.0 && Uniformity <= 1.0))
            {
                error = string.Format(
                    CultureInfo.InvariantCulture,
                    "均匀性 U 必须在 0–1 之间（当前 {0:0.####}）。",
                    Uniformity);
                return false;
            }

            if (!(Efficiency >= 0.0 && Efficiency <= 1.0))
            {
                error = string.Format(
                    CultureInfo.InvariantCulture,
                    "效率 η 必须在 0–1 之间（当前 {0:0.####}）。",
                    Efficiency);
                return false;
            }

            if (!(LensDiameterMm > 0.0))
            {
                error = string.Format(
                    CultureInfo.InvariantCulture,
                    "透镜口径 LensDiameterMm 必须大于 0（当前 {0:0.####} mm）。",
                    LensDiameterMm);
                return false;
            }

            if (!(LensHeightMm > 0.0))
            {
                error = string.Format(
                    CultureInfo.InvariantCulture,
                    "透镜高度 LensHeightMm 必须大于 0（当前 {0:0.####} mm）。",
                    LensHeightMm);
                return false;
            }

            // 面域（指定距离照度场）必须给出接收面距离；远场角域不要求。
            if (SpotType == SpotType.NearField)
            {
                if (!DistanceM.HasValue)
                {
                    error = "光斑类型为面域（NearField）时必须填写接收面距离 DistanceM（单位：米）。";
                    return false;
                }

                if (!(DistanceM.Value > 0.0))
                {
                    error = string.Format(
                        CultureInfo.InvariantCulture,
                        "接收面距离 DistanceM 必须大于 0（当前 {0:0.####} m）。",
                        DistanceM.Value);
                    return false;
                }
            }

            // 控制点为空表示稍后生成，允许；一旦给出则数量必须落在 5–32。
            if (ControlPoints != null && ControlPoints.Count > 0 &&
                (ControlPoints.Count < MinControlPoints || ControlPoints.Count > MaxControlPoints))
            {
                error = string.Format(
                    CultureInfo.InvariantCulture,
                    "控制点数量必须在 {0}–{1} 之间（当前 {2} 个）；为空表示稍后由算法生成。",
                    MinControlPoints,
                    MaxControlPoints,
                    ControlPoints.Count);
                return false;
            }

            if (ControlPoints != null)
            {
                for (int i = 0; i < ControlPoints.Count; i++)
                {
                    if (ControlPoints[i] == null)
                    {
                        error = string.Format(
                            CultureInfo.InvariantCulture,
                            "控制点列表第 {0} 个元素为 null，请检查数据来源。",
                            i);
                        return false;
                    }
                }
            }

            error = null;
            return true;
        }
    }
}
