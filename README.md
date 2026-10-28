# 奶豆 Windows 桌面美化主题 (MilkBean Desktop Theme)

> 可安装、可恢复的 Windows 桌面主题商品 MVP  
> 核心角色：奶豆 3D 卡通形象

**仓库：** https://github.com/1parado/MilkBean-Desktop-Theme  
**目标平台：** Windows 10 / Windows 11 x64  
**产品版本：** MVP 1.0  

---

## 当前状态

| 阶段 | 状态 |
| --- | --- |
| 项目文档 | ✅ PRD / TECH_SPEC / ARCHITECTURE / TEST_PLAN |
| Rainmeter 皮肤 | ✅ 时钟 / 日历骨架 |
| C# 安装器核心 | ✅ 服务层 + 向导 UI |
| 本机 Windows 编译实测 | ⏳ 待进行 |
| 壁纸 / ICO 资源 | ⏳ 需补齐 |
| 可分发安装包 | ⏳ 待进行 |

---

## 构建与运行

```powershell
git clone https://github.com/1parado/MilkBean-Desktop-Theme.git
cd MilkBean-Desktop-Theme

# 需要 .NET 8 SDK + Windows
dotnet build MilkBean.Desktop.Theme.sln -c Release
dotnet run --project src/MilkBean.Installer

# 发布
dotnet publish src/MilkBean.Installer/MilkBean.Installer.csproj -c Release -r win-x64 --self-contained -o publish
```

主题资源会在构建时复制到输出目录 `themes/Desktop-01/`。

---

## 已实现的服务

- `EnvironmentService` — 系统 / Rainmeter 检测
- `WallpaperService` — 壁纸读取、设置、恢复
- `FolderIconService` — desktop.ini 图标
- `RainmeterService` — 皮肤部署与激活
- `NotesService` — 本地 JSON 便签
- `BackupService` / `RestoreService`
- `ThemeApplyOrchestrator` — 应用流程与回滚

---

## 文档

- [PRD.md](docs/PRD.md)
- [TECH_SPEC.md](docs/TECH_SPEC.md)
- [ARCHITECTURE.md](docs/ARCHITECTURE.md)
- [TEST_PLAN.md](docs/TEST_PLAN.md)
- [CHANGELOG.md](docs/CHANGELOG.md)

---

## 下一步（建议）

1. 在 Windows 上 `dotnet build` 并跑通安装器
2. 补齐壁纸 JPG（1920/2560/3840）与文件夹 ICO
3. 上传奶豆角色 PNG 到 `themes/Desktop-01/Assets/`
4. 按 `TEST_PLAN.md` 做技术验证
5. 完善独立便签窗口 UI

---

*Made with love for MilkBean fans*
