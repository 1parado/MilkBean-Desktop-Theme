# 奶豆桌面主题 - 架构说明

**项目代号：** MilkBean Desktop Theme  
**版本：** MVP 1.0  

---

## 1. 系统分层

```
Presentation (WPF Installer UI)
        ↓
Application Services (ThemeApply / Restore Orchestrator)
        ↓
Domain Services (Environment, Wallpaper, Rainmeter, FolderIcon, Notes, Backup, Restore, Logging, Manifest)
        ↓
Infrastructure (Win32 P/Invoke, File System, JSON, Process)
```

## 2. 数据流（主题应用）

用户选择主题 → 读 theme.json → 环境检查 → 生成备份 → 设壁纸 → 部署 Rainmeter 皮肤 → 启动便签 → （可选）应用文件夹图标 → 写状态与日志。失败时按已完成步骤反向回滚。

## 3. 本地数据布局

```
%LOCALAPPDATA%\MilkBean\
├── App\
├── Themes\Desktop-01\
├── Config\ (app-state.json, notes.json, user-preferences.json)
├── Backups\backup-YYYYMMDD-HHMMSS\
└── Logs\
```

## 4. 关键设计决策

| 决策点 | 选择 | 理由 |
| --- | --- | --- |
| UI 框架 | WPF | 成熟、API 直接、MVP 成本低 |
| 桌面组件 | Rainmeter + 独立便签窗口 | 时钟/日历用 Rainmeter；便签保证可编辑 |
| 配置格式 | JSON | 人类可读、易扩展 |
| 权限模型 | 尽量用户权限 | 减少 UAC |
| 备份粒度 | 分项备份 | 部分失败不影响全部恢复 |

## 5. 扩展点

- 新增主题：增加 themes/xxx 与 theme.json
- 新增组件类型：扩展 widgets.type 与对应 Service

---

*架构说明结束。*
