# tools/ — 构建与诊断脚本目录（占位）

> P0 启动基线阶段，环境检查脚本位于启动基线包 `P0_启动基线/tools/`（`check_env.ps1` 等）。本目录用于初始化 Git 仓库后承载**随代码维护**的脚本。

计划收录：

- `构建.ps1` / `build.ps1`：一键 restore / build / test（Windows + VS 2022 Build Tools + .NET SDK 8）
- `检查环境.ps1` / `check_env.ps1`：VS / MSBuild / SDK / LightTools / SolidWorks / Guipc 检测
- `生成报告.py`：仿真指标汇总与报告生成（后续阶段）

约束：脚本可重复执行、不改系统状态；不启动 LightTools / SolidWorks 自动化（必须人工守候）；PowerShell 至少兼容 pwsh 7。
