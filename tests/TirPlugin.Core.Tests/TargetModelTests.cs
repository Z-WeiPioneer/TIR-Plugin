using NUnit.Framework;
using TirPlugin.Core.Models;

namespace TirPlugin.Core.Tests
{
    /// <summary>
    /// TargetModel 默认值与输入校验测试（P0 数据契约门禁）。
    /// 规则来源：《TIR 插件功能与接口规格 V1.1》输入参数表 + P0 启动基线任务卡。
    /// </summary>
    [TestFixture]
    public sealed class TargetModelTests
    {
        /// <summary>默认值必须合法，且与规格 V1.1 的默认值一致。</summary>
        [Test]
        public void TargetModel_Defaults_AreValidAndMatchSpec()
        {
            TargetModel model = new TargetModel();

            string error;
            Assert.That(model.Validate(out error), Is.True, error);

            Assert.That(model.BeamAngleDeg, Is.EqualTo(30.0), "默认光束角应为 30°");
            Assert.That(model.SpotType, Is.EqualTo(SpotType.FarField), "默认光斑类型应为远场");
            Assert.That(model.DistanceM, Is.Null, "远场默认不填距离");
            Assert.That(model.Uniformity, Is.EqualTo(0.80), "默认均匀性应为 0.80");
            Assert.That(model.Efficiency, Is.EqualTo(0.85), "默认效率应为 0.85");
            Assert.That(model.SourceWidthMm, Is.EqualTo(2.0), "默认发光面宽应为 2 mm");
            Assert.That(model.SourceHeightMm, Is.EqualTo(2.0), "默认发光面高应为 2 mm");
            Assert.That(model.MaterialName, Is.EqualTo("PMMA"), "默认材料应为 PMMA");
            Assert.That(model.RefractiveIndex, Is.EqualTo(1.49), "PMMA 折射率应为 1.49");
            Assert.That(model.LensDiameterMm, Is.EqualTo(34.0), "默认口径应为 34 mm");
            Assert.That(model.LensHeightMm, Is.EqualTo(18.0), "默认高度应为 18 mm");
            Assert.That(model.ControlPoints, Is.Empty, "控制点默认空，稍后生成");
            Assert.That(model.Seed, Is.EqualTo(20261121), "默认 seed 应为 20261121");
        }

        /// <summary>光束角越界（&gt; 90°）必须校验失败。</summary>
        [Test]
        public void TargetModel_BeamAngleOutOfRange_Fails()
        {
            TargetModel model = new TargetModel();
            model.BeamAngleDeg = 100.0;

            string error;
            Assert.That(model.Validate(out error), Is.False);
            Assert.That(error, Does.Contain("光束角"));
        }

        /// <summary>面域（NearField）缺少接收距离必须校验失败。</summary>
        [Test]
        public void TargetModel_NearFieldWithoutDistance_Fails()
        {
            TargetModel model = new TargetModel();
            model.SpotType = SpotType.NearField;
            model.DistanceM = null;

            string error;
            Assert.That(model.Validate(out error), Is.False);
            Assert.That(error, Does.Contain("距离"));
        }

        /// <summary>控制点数量 33 个（超过上限 32）必须校验失败。</summary>
        [Test]
        public void TargetModel_ThirtyThreeControlPoints_Fails()
        {
            TargetModel model = new TargetModel();
            for (int i = 0; i < 33; i++)
            {
                model.ControlPoints.Add(new CurvePoint { ThetaDeg = i * 2.0, R = 1.0 });
            }

            string error;
            Assert.That(model.Validate(out error), Is.False);
            Assert.That(error, Does.Contain("控制点"));
        }
    }
}
