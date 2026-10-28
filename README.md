# 奶豆 Windows 桌面美化主题 (MilkBean Desktop Theme)

> 可安装、可恢复的 Windows 桌面主题商品 MVP  
> 核心角色：奶豆 3D 卡通形象

**仓库：** https://github.com/1parado/MilkBean-Desktop-Theme  
**目标平台：** Windows 10 / Windows 11 x64  
**产品版本：** MVP 1.0  

---

## 产品简介

为喜欢可爱、治愈桌面的 Windows 用户提供一套以「奶豆」为主题的完整桌面美化方案：

- 一键应用壁纸、时钟、日历、便签
- 奶豆风格文件夹图标
- 安装前自动备份，支持一键恢复原状
- 不依赖复杂配置，普通用户也能轻松使用

本项目不是单纯的壁纸包，也不是需要用户自己折腾的 Rainmeter 皮肤集合，而是具备安装、应用、管理和恢复能力的数字主题商品。

---

## 当前状态

| 阶段 | 状态 |
| --- | --- |
| 项目侦察与文档 | ✅ 已完成 |
| 技术验证方案 | ✅ 已完成 |
| 仓库骨架与主题配置 | ✅ 已完成 |
| Rainmeter 皮肤原型 | 🚧 骨架已就绪 |
| C# 安装器核心实现 | 🚧 接口骨架 |
| 本机 Windows 编译与实测 | ⏳ 待进行 |
| 可分发安装包 | ⏳ 待进行 |

---

## 文档

| 文档 | 说明 |
| --- | --- |
| [PRD.md](docs/PRD.md) | 产品需求文档（完整） |
| [TECH_SPEC.md](docs/TECH_SPEC.md) | 技术规格说明书 |
| [ARCHITECTURE.md](docs/ARCHITECTURE.md) | 架构说明 |
| [TEST_PLAN.md](docs/TEST_PLAN.md) | 测试计划与验收标准 |
| [CHANGELOG.md](docs/CHANGELOG.md) | 变更记录 |

---

## 目录结构

```
MilkBean-Desktop-Theme/
├── docs/                          # 产品与技术文档
├── src/
│   ├── MilkBean.Installer/        # WPF 主题安装器（C#）
│   └── MilkBean.Notes/            # 独立便签窗口（可选）
├── themes/
│   └── Desktop-01/                # 首发主题资源
│       ├── theme.json
│       ├── Wallpaper/
│       ├── Rainmeter/Skins/MilkBean/
│       ├── Icons/
│       └── Assets/                # 含奶豆角色原图
├── scripts/
└── tests/
```

---

## 快速开始（开发者）

### 环境要求

- Windows 10 / 11 x64
- .NET 8 SDK 或更高（推荐 LTS）
- Visual Studio 2022（推荐）或 VS Code + C# 扩展
- Rainmeter（可选，安装器会引导）

### 本机技术验证（优先完成）

在编码完整安装器前，请先在本机验证：

1. 壁纸读取 / 设置 / 恢复
2. 文件夹图标（desktop.ini）应用与恢复
3. Rainmeter 皮肤加载与卸载
4. 便签独立窗口编辑与持久化

详见 `docs/TEST_PLAN.md` 中的「技术验证检查清单」。

### 构建（规划）

```powershell
# 进入解决方案目录后
dotnet build src/MilkBean.Installer/MilkBean.Installer.csproj
dotnet publish src/MilkBean.Installer/MilkBean.Installer.csproj -c Release -r win-x64 --self-contained
```

---

## 设计原则（摘自 PRD）

1. 先验证再开发
2. 最小化系统修改
3. 默认保护用户数据
4. 失败必须可诊断
5. 不把静态效果冒充真实功能
6. 不静默安装外部依赖
7. 不覆盖用户原有 Rainmeter 配置与无关文件夹图标

---

## 角色素材

奶豆形象已包含在：

- `themes/Desktop-01/Assets/milkbean-character.png`
- `src/MilkBean.Installer/Assets/milkbean-preview.png`

必须保留黄色圆润身体、大眼睛、小圆嘴与 3D 玩具质感，不得随意改为扁平或其他风格。

---

## 许可证与商业化

本仓库当前为开发与试售准备阶段。  
正式商品发布时将附带明确的使用与分发授权说明。

建议试售商品名：**奶豆桌面主题 01 · 日常陪伴版**（¥6.9 待验证）。

---

## 贡献与反馈

当前由作者维护。欢迎在 Issues 中反馈安装失败、恢复失败或视觉问题。

---

*Made with 💛 for MilkBean fans*
