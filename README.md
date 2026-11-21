# TirPlugin — 指定光束角 TIR 透镜自动设计插件（P0 骨架）

> **P0 启动基线 · 最小可构建骨架。当前未验证构建：** 本机没有 Visual Studio / MSBuild、没有 .NET SDK 与 Git；下列命令按"环境就绪后目标状态"编写，**尚未实际执行**。在实跑并记录结果前，不得声称"构建通过 / 测试通过"。

## 这是什么

指定光束角（2θ½）、圆形光斑的 TIR 透镜自动设计插件，目标平台 Windows x64 / .NET Framework 4.8 / WinForms（MVP）/ C# 7.3。

本仓库当前只包含 P0 启动基线所需的最小骨架：

- `src/TirPlugin.Core/`：纯 .NET 类库（数据契约 `TargetModel`、`CurvePoint`，后续放光学算法），**不引用 WinForms / COM / Guipc**。
- `tests/TirPlugin.Core.Tests/`：NUnit 3 测试（net48 / x64），覆盖 `TargetModel` 默认值与输入校验规则。

## 项目结构

```text
repo/
├─ TirPlugin.sln              # VS2022 解决方案（Debug/Release × Any CPU/x64，Any CPU 映射到 x64）
├─ Directory.Build.props      # 统一：C# 7.3 / net48 / x64 / Deterministic
├─ global.json                # 固定 .NET SDK 8.0.x（rollForward latestFeature）
├─ src/
│  └─ TirPlugin.Core/
│     ├─ TirPlugin.Core.csproj
│     └─ Models/              # CurvePoint.cs、TargetModel.cs
├─ tests/
│  └─ TirPlugin.Core.Tests/
│     ├─ TirPlugin.Core.Tests.csproj
│     └─ TargetModelTests.cs
├─ docs/                      # 架构、ADR、数据契约、验收文档（占位）
├─ tools/                     # 构建/诊断脚本（占位）
└─ .github/workflows/ci.yml   # GitHub Actions：windows-latest，restore/build/test
```

## 先装什么（一次性）

1. **Git for Windows** — <https://git-scm.com/download/win>
2. **.NET SDK 8.0.x** — <https://dotnet.microsoft.com/download/dotnet/8.0>（`dotnet --list-sdks` 验证）
3. **VS 2022 Build Tools** — .NET 桌面工作负载 + .NET Framework 4.8 targeting pack（提供 net48 参考程序集与 MSBuild）

完整 30 分钟安装与首次构建路径见 [`../ci/README.md`](../ci/README.md)；协作与代码规范见 [`../AGENTS.md`](../AGENTS.md)（初始化 Git 后复制为仓库根 `AGENTS.md`）。

## 构建与测试

在仓库根执行（CI 同款，推荐）：

```powershell
$env:COMPLUS_LoadFromRemoteSources = "1"   # D: 工作区必需；CI 已内置
dotnet restore tests/TirPlugin.Core.Tests/TirPlugin.Core.Tests.csproj
dotnet build   tests/TirPlugin.Core.Tests/TirPlugin.Core.Tests.csproj -c Release
dotnet test    tests/TirPlugin.Core.Tests/TirPlugin.Core.Tests.csproj -c Release --logger "trx;LogFileName=test-results.trx" --results-directory TestResults
```

解决方案方式（VS 2022 打开 `TirPlugin.sln` 并选 x64，或命令行）：

```powershell
$env:COMPLUS_LoadFromRemoteSources = "1"
dotnet restore TirPlugin.sln
dotnet build   TirPlugin.sln -c Release
dotnet test    TirPlugin.sln -c Release
```

期望：`TargetModelTests` 的 4 个测试全部通过；`TestResults/test-results.trx` 生成。

## 当前验证状态

| 项 | 状态 |
| --- | --- |
| 工程文件静态解析（XML / JSON / YAML）与 sln 结构检查 | ✅ 已执行（P0-01，静态检查） |
| 本机 `dotnet restore` | ✅ 已执行：两份项目均还原成功（2026-11-21 22:02） |
| 本机 `dotnet build`（测试项目 + sln） | ✅ 已执行：0 警告 0 错误（2026-11-21 22:02 / 22:05） |
| 本机 `dotnet test` | ✅ 已执行：4/4 通过（需 `COMPLUS_LoadFromRemoteSources=1`，2026-11-21 22:03 / 22:05） |
| GitHub Actions CI | ✅ **已通过**：run [44916173463](https://github.com/Z-WeiPioneer/TIR-Plugin/actions/runs/44916173463)，含“测试数 ≥4/4”守卫；首次 run [44914649453](https://github.com/Z-WeiPioneer/TIR-Plugin/actions/runs/44914649453) 也成功 |
| 补充源码级预验证（**不是**构建验收） | ⚠️ 已由正式构建/测试取代；保留记录见 [`../ci/P0-01_验证记录.md`](../ci/P0-01_验证记录.md) |

> 注意：本工作区在 D: 盘时，.NET Framework 会把 `Assembly.LoadFrom` 判为远程，未设置 `COMPLUS_LoadFromRemoteSources=1` 时会出现“未能从 NUnit3.TestAdapter.dll 加载扩展”，这是测试宿主环境问题，不是测试代码失败。CI 工作流已内置该变量。

## 约定与红线

- 协作约定、AI 任务规则、PR 检查清单：`../AGENTS.md`。
- 禁止提交二进制 / 许可 / 密钥；禁止自动操作 LightTools / SolidWorks；未经许可的反编译产物不得进入仓库或安装包（详见 AGENTS.md 第 6 节）。
- 未验证项必须标注"本机未验证"，禁止把计划写成已完成。
