# AGENTS.md — 指定光束角 TIR 透镜自动设计插件

> 本文件是本仓库的最高优先级协作约定（AI 与人类共同遵守）。**先读本文件，再读 `docs/`，然后动手。**
>
> 位置说明：本文件当前位于 P0 启动基线包（`P0_启动基线/AGENTS.md`）；项目代码仓库根为 `P0_启动基线/repo/`，初始化 Git 时应复制到 `repo/AGENTS.md` 并随代码维护。下文命令如无特别说明，均在仓库根（`repo/`）下执行。

## 1. 项目一句话与当前阶段

**项目**：指定光束角（2θ½）、圆形光斑的 TIR 透镜自动设计插件 —— 双模输入（快捷参数 / 配光曲线拖拽）→ 可行性（etendue）→ 一阶布局 → 角度映射 → 2D 母线 → 快速追迹 → 内环优化 → LightTools 外环 ILC 校验 → SolidWorks 建模/鳞甲 → 报告导出。平台：Windows x64，.NET Framework 4.8，WinForms（MVP），C# 7.3。

**事实基线（不得改写）**：
- 已安装：LightTools 8.7.0、SolidWorks 2025 SP0；.NET Framework 4.8（Release 533509）；.NET SDK 8.0.428；VS 2022 Build Tools（MSBuild 17.14.60）；Git 2.56.1。
- 本机已完成 `dotnet restore/build/test`：Core + Tests 构建 0 警告 0 错误，4/4 测试通过（2026-11-21 22:06）；CI 尚未执行（无远端仓库）。
- **本工作区在 D: 盘时，.NET Framework 会把 `Assembly.LoadFrom` 判为远程**，测试宿主需设置 `COMPLUS_LoadFromRemoteSources=1`；CI 已内置该环境变量。未设置时表现为“未从 NUnit3.TestAdapter.dll 加载扩展”，不是测试代码失败。
- Guipc 明文资产已获二次开发许可，下一阶段直接封装；**P0 启动基线阶段不引用 Guipc DLL**，只用接口与数据契约占位。
- LightTools / SolidWorks 的自动化操作必须人工守候、串行执行；P0 阶段禁止自动操作。

**当前阶段：P0 启动基线**（P0-01 ~ P0-05）：
1. P0-01 仓库骨架与 AGENTS.md（本文件 + `repo/`）
2. P0-02 环境与许可矩阵、环境检查脚本（`docs/` + `tools/`）
3. P0-03 黄金案例（`golden_cases/`）
4. P0-04 ADR、风险与决策日志、P0 验收清单（`docs/`）
5. P0-05 CI 骨架与构建说明（`ci/` + `repo/.github/workflows/ci.yml`）

**门禁**：P0 交付物齐备，且环境检查、`restore / build / test` 实际通过；CI 首跑需在有远端仓库后回填。未执行的项一律标记"待验证"。

## 2. 目录结构

启动基线包（`P0_启动基线/`）：
- `AGENTS.md` 本文件
- `repo/` 代码仓库骨架（将来复制/初始化为 Git 仓库根）
- `docs/` 环境与许可矩阵、ADR、风险与决策日志、P0 验收清单
- `golden_cases/` 10° / 30° / 60° 黄金案例
- `tools/` 环境检查等脚本
- `ci/` 本地与 CI 构建说明

代码仓库（`repo/`）：
```text
repo/
├─ TirPlugin.sln              # VS2022 解决方案（x64）
├─ Directory.Build.props      # 统一：C# 7.3 / net48 / x64 / Deterministic
├─ global.json                # 固定 .NET SDK 8.0.x（rollForward latestFeature）
├─ src/
│  └─ TirPlugin.Core/         # 纯 .NET 类库：数据契约 + 光学算法（无 WinForms/COM/Guipc）
│     └─ Models/              # TargetModel、CurvePoint 等数据契约
├─ tests/
│  └─ TirPlugin.Core.Tests/   # NUnit 3；net48；x64
├─ docs/                      # 架构、ADR、数据契约、验收、用户手册
├─ tools/                     # 构建/诊断脚本
└─ .github/workflows/ci.yml   # GitHub Actions：windows-latest，restore/build/test
```

分层约束：`TirPlugin.Core` 不引用 WinForms / COM / Guipc；UI 与 LT/SW 桥接层后续放独立项目，通过接口依赖 Core。

## 3. 构建 / 测试命令

一次性前置（详见 `ci/README.md`）：VS 2022 Build Tools（.NET 桌面工作负载 + .NET Framework 4.8 targeting pack）、.NET SDK 8.0.x、Git。

单项目方式（CI 同款，推荐）：
```powershell
$env:COMPLUS_LoadFromRemoteSources = "1"   # D: 工作区必需；CI 已内置
dotnet restore tests/TirPlugin.Core.Tests/TirPlugin.Core.Tests.csproj
dotnet build   tests/TirPlugin.Core.Tests/TirPlugin.Core.Tests.csproj -c Release
dotnet test    tests/TirPlugin.Core.Tests/TirPlugin.Core.Tests.csproj -c Release --logger "trx;LogFileName=test-results.trx" --results-directory TestResults
```

解决方案方式（VS 2022 打开 `TirPlugin.sln`，平台选 x64，或命令行）：
```powershell
$env:COMPLUS_LoadFromRemoteSources = "1"
dotnet restore TirPlugin.sln
dotnet build   TirPlugin.sln -c Release
dotnet test    TirPlugin.sln -c Release
```

产物与约定：`TestResults/*.trx`（CI 上传 artifact）；`bin/`、`obj/` 不入库。
> 环境未就绪时不得跳过测试直接合并；只允许把"未验证"写清楚后交人工评审，并在环境就绪后补跑。

## 4. 代码规范

- **语言/平台**：C# 7.3（`Directory.Build.props` 锁定 `LangVersion`）；`net48`；`x64`；`Deterministic=true`；禁止 nullable 引用类型。
- **禁用 C# 8+ 语法**：nullable 引用类型、文件作用域命名空间（file-scoped namespace）、using 声明、switch 表达式、record、init 访问器、目标类型 `new`、范围/索引等。
- **格式**：4 空格缩进（见 `.editorconfig`）；UTF-8；行宽建议 ≤ 120；文件末尾留空行；`using` 排序（System 在前）。
- **命名**：类型/公共成员 PascalCase；接口 `I` 前缀；局部变量/参数 camelCase；私有字段 camelCase（团队统一，不加下划线）；常量 PascalCase；异步方法 `Async` 后缀；测试方法 `被测对象_场景_期望`（英文、下划线分隔，如 `TargetModel_Defaults_AreValidAndMatchSpec`）。
- **文件组织**：一个文件一个主类型；同一数据契约的紧密枚举可同文件（如 `TargetModel.cs` 中的 `SpotType`）；namespace 与目录对应（如 `TirPlugin.Core.Models`）。
- **异常**：库层不吞异常、不用异常表达业务校验失败；输入非法用 `Validate(out string error)` 返回错误消息；COM/UI/IO 边界统一捕获、记录并转换为可读错误；禁止空 `catch`。
- **日志**：统一日志抽象（P0 可先用 Console/文件实现）；每次运行记录 `run_id`、`seed`、版本、关键参数、耗时；日志与报告不得包含密钥/许可信息。
- **依赖**：新增 NuGet 必须在 PR 说明用途、版本、许可；Core 禁止引入 UI/COM/Guipc 依赖。
- **测试**：新功能先写失败测试再实现；bug 修复必须先补回归测试；纯逻辑（Validate/映射/数学）必须有测试；COM/LT/SW 依赖用接口抽象 + 假实现，CI 不跑真实软件。
- **数值与可复现**：固定 `Seed`；禁止依赖隐式全局状态、当前时间、随机顺序；浮点比较使用容差。

## 5. AI 任务规则

1. **一任务一分支 / 一 agent**：分支名 `task/<任务编号>-<简述>`（如 `task/p0-01-repo-bootstrap`）；同一时间只允许一个 agent 写同一文件（写作用域见任务卡）。
2. **开工前必读**：本 `AGENTS.md` → `docs/`（ADR、数据契约、验收）→ 任务卡；需求有歧义先问，不自行脑补。
3. **写作用域**：只在任务卡 `writeScopes` 内写文件；需要改作用域外文件时先在任务卡/PR 说明并拆任务，不擅自扩大。
4. **先测试后实现**：提交顺序为失败测试 → 实现 → 通过测试；PR 描述必须附：执行命令、通过/失败数、是否本机执行。
5. **小 PR**：单 PR 变更 < 400 行（不含自动生成文件/文档）；超过就拆。
6. **不得改动**：`*.Designer.cs`、自动生成的 `.resx` 资源代码、`src/*/lib/**`、证书/密钥/token；不得为"让 CI 变绿"修改 CI 或 `Directory.Build.props`（必须单独评审）。
7. **未验证标注**：任何未实际执行的构建/测试/仿真，必须在文档和 PR 中写"本机未验证"，禁止夸大成"已通过"。
8. **交付物**：任务完成时在任务卡写：改动的文件、执行的检查命令与结果、未验证项、风险；并给 lead 发 ≤300 字简报。

## 6. 禁止事项（红线，违反即回滚）

- 不提交二进制/大文件：`*.dll`、`*.exe`、`*.pdb`、`*.nupkg`、Guipc 明文资产、许可证文件、密钥/token、证书；确需第三方依赖走 NuGet 或文档说明的人工安装步骤。
- 不在 CI/无人守候的脚本里启动、操作 LightTools / SolidWorks；不自动向生产实例发送 COM 命令。
- 未经许可的反编译/脱壳产物不得进入仓库、安装包或交付物；Guipc 复用只限已获二次开发许可的接口调用，不绕过任何许可校验。
- 不改动本机已安装软件目录；不做版本降级/破解/注入。
- 不伪造数据：光学指标必须来自实际仿真/测量，记录版本、seed、光线数、`run_id`；黄金案例 baseline 未测前只能标 `pending`。
- 不擅自删除/覆盖既有资料（`TIR LENS市面插件逆向工程/`、`_本轮评估/`、`学术依据/`、`立项评估与技术路线/` 等为只读参考）。

## 7. PR 检查清单（复制到 PR 描述）

- [ ] 编译：`dotnet build` 通过（注明本机/CI；未执行则写"本机未验证"）
- [ ] 测试：`dotnet test` 通过，附命令与通过/失败数
- [ ] 语言：C# 7.3 兼容、无 nullable 语法、无新增编译警告
- [ ] 线程/COM：WinForms 仅 UI 线程访问控件；COM 调用串行、带超时/重试/错误日志（涉及桥接时必填）
- [ ] 日志：`run_id`/`seed`/参数/错误可追溯，无敏感信息
- [ ] UI：WinForms 改动附截图（至少 100% 与 150% DPI 之一），无布局截断/控件泄漏
- [ ] 性能：新增热点路径有耗时记录或预算说明（如内环单步、1000 次迭代）
- [ ] 文档：`AGENTS.md` / `docs/` / `README` 同步；未验证项已标注
- [ ] 合规：无二进制/密钥/许可文件；P0 阶段无 Guipc DLL 引用
- [ ] 追溯：PR < 400 行；一个任务/分支；关联任务卡编号

## 8. 参考（只读，勿改）

- `立项评估与技术路线/技术路径与功能规划/TIR插件功能与接口规格_V1.1.xlsx`
- `立项评估与技术路线/开发实施/TIR插件开发实施计划.docx`
- `_本轮评估/route_v11_text.txt`（V1.1 文字版路线）
- `P0_启动基线/docs/`（环境矩阵、ADR、风险、验收）、`P0_启动基线/golden_cases/`（黄金案例）
