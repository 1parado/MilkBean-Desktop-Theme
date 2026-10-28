# 奶豆桌面主题 - 技术规格说明书

**项目代号：** MilkBean Desktop Theme  
**版本：** MVP 1.0  
**文档版本：** v1.0  

---

## 10. 技术架构

### 10.1 总体架构

```
MilkBean Desktop Theme
│
├── Theme Installer
│   ├── Environment Check
│   ├── Theme Apply
│   ├── Backup Manager
│   ├── Restore Manager
│   └── Uninstaller
│
├── Theme Resources
│   ├── Wallpaper
│   ├── Rainmeter Skins
│   ├── Folder Icons
│   └── Theme Manifest
│
├── Desktop Widgets
│   ├── Clock
│   ├── Calendar
│   └── Notes
│
└── Local Data
    ├── User Config
    ├── Notes Data
    ├── Backup Records
    └── Logs
```

### 10.2 技术选型

| 模块 | 推荐技术 | 原因 |
| --- | --- | --- |
| Windows 安装器 | C# + .NET 8/10 LTS + WPF | Windows API 调用成熟 |
| 壁纸管理 | Windows 原生 API / Win32 / IDesktopWallpaper | 直接设置系统壁纸 |
| 桌面组件 | Rainmeter | 复用成熟皮肤与布局能力 |
| 时钟与日历 | Rainmeter 原生皮肤 | 实现简单、资源消耗低 |
| 便签编辑 | 优先独立轻量 WPF 窗口 | 保证真实文本编辑与可靠保存 |
| 图标资源 | ICO 多尺寸文件 | 兼容 Windows 文件夹图标 |
| 本地配置 | JSON | 易读、易调试 |
| 便签持久化 | JSON 文件 | 单机轻量场景 |
| 日志 | .NET 内置日志或轻量文件日志 | 方便定位安装失败 |
| 打包 | 自包含发布包或 MSIX | 简化分发 |

**建议：** 优先采用 C# + WPF + Rainmeter 完成 MVP。

---

## 11. Rainmeter 集成规格

Rainmeter 负责渲染桌面组件；安装器负责检测、部署、加载/卸载本产品皮肤、备份与恢复。

皮肤结构：
```
Skins/MilkBean/
├── Clock/Clock.ini
├── Calendar/Calendar.ini
├── Notes/Notes.ini
└── Shared/Variables.inc
```

依赖处理：检测 → 提示安装（需用户同意）→ 部署皮肤 → 验证加载。禁止静默安装。

---

## 12. 主题配置文件规范

每个主题使用 `theme.json`（见 `themes/Desktop-01/theme.json`）。

- schemaVersion 用于未来升级
- 路径必须规范化与合法性检查
- 禁止路径逃逸

---

## 13–18. 应用流程、备份恢复、图标、便签、服务划分与错误处理

详见仓库完整 TECH_SPEC 与 ARCHITECTURE 文档。核心原则：

1. 应用非原子，必须分步记录与可回滚
2. 备份分项（壁纸 / Rainmeter / 文件夹图标）
3. 恢复状态：Success / Skipped / Failed / NotSupported
4. 便签数据本地 JSON，临时文件替换写入
5. 服务：Environment / Manifest / Backup / Wallpaper / Rainmeter / FolderIcon / Notes / Restore / Logging

---

## 19. 测试与验收

见 `docs/TEST_PLAN.md`。MVP 发布前必须满足 10 条验收标准。

---

*技术规格结束。*
