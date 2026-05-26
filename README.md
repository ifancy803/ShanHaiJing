# 《异兽拼图册》—— 
# 基于AI全链路赋能的《山海经》文化认知交互系统

## 项目简介 (中文)

《异兽拼图册》是一款基于Unity引擎、深度融合AI全链路的《山海经》文化认知交互系统。项目以智能拼图为核心玩法，内置九大异兽百科知识库，技术栈包括Unity、C#、Addressable资源管理、事件中心解耦架构，以及AI生成艺术（AI美术/音乐）、AI动态难度调整和AI文化Agent助手。亮点在于：实现“AI美术+AI音乐+AI代码”全流程辅助开发，支持基于玩家行为的自适应难度调节，提供自然语言交互的文化智能体，将晦涩古文转译为沉浸式拼图解谜与数字艺术体验，形成“玩-学-藏”闭环。

## English Description

*The Classic of Mountains and Seas Puzzle Album* is a cultural cognition interactive system of *The Classic of Mountains and Seas* deeply integrated with full-chain AI, developed on the Unity engine. The project features smart jigsaw puzzles as its core gameplay, with a built-in knowledge base of nine mythical beasts. The technology stack includes Unity, C#, Addressable asset management, event hub decoupling architecture, as well as AI-generated art (AI art/music), AI dynamic difficulty adjustment, and an AI cultural agent assistant. Highlights include: full-process assisted development workflow of “AI art + AI music + AI code”, adaptive difficulty adjustment based on player behavior, an intelligent cultural agent for natural language interaction, and the transformation of obscure ancient texts into immersive puzzle-solving and digital art experiences, forming a “play-learn-collect” closed loop.

## 技术栈

- **游戏引擎**：Unity (C#)
- **资源管理**：Addressable Asset System
- **架构模式**：泛型事件中心解耦通信、无限循环列表优化
- **AI应用层**：
  - AI美术：国风水墨+神话幻想风格（AI生成艺术工具）
  - AI音乐：异兽主题旋律与交互音效（AI辅助生成）
  - AI代码：AI编程助手辅助完成高效代码架构
  - AI动态难度：基于玩家完成时间与失误率的自适应难度调节
  - AI Agent：自然语言对话式文化智能体（山海经文化助手）

## 核心功能

| 模块 | 描述 |
|------|------|
| 智能拼图系统 | 支持9/16/25块多难度，拖拽吸附与自动拼合检测，AI动态调整关卡复杂度，提供“心流通道”最优体验 |
| 神兽百科知识库 | 内置九大异兽，完成拼图解锁高清插画与百科卡片（名称、出处、形态、传说），支持点击部位显示解说泡泡 |
| AI文化Agent | 内置智能体，玩家可随时以自然语言查询神兽信息、传说背景，获得智能导览与解答 |
| 全流程AI内容生产 | 覆盖美术、音乐、代码辅助开发，极大提升创作效率与品质 |

## 项目亮点

### 智能性（AI技术深度应用）
- **内容生产端**：形成“AI美术 + AI音乐 + AI代码”的全链路辅助开发工作流。
- **用户体验端**：实现基于实时行为数据的动态难度调整 + 自然语言交互的AI文化助手。

### 文化内涵与审美体验
- **文化转译**：将晦涩古文转化为可视、可听、可交互的拼图与百科，从被动阅读转向主动探索。
- **东方视觉**：以“宣纸肌理 + 古铜配色”为UI基调，融合国风水墨与AI生成艺术，每帧画面皆为数字艺术品。

### 技术创新性（学科融合）
- **跨学科融合**：深度融合计算机科学（AI算法、Unity开发）、设计学（UI/UX、国风美术）、音乐学与民俗学（《山海经》文化）。
- **高性能架构**：泛型事件中心解耦、Addressable资源管理，确保低性能设备流畅运行。

## 运行与开发

### 环境要求
- Unity 2021.3 LTS 或更高版本
- 支持平台：Windows / macOS / iOS / Android

### 快速开始
1. 克隆仓库
   ```bash
   git clone https://github.com/your-repo/yishou-puzzle-album.git
```

1. 使用Unity打开项目根目录
2. 通过Addressable Groups配置资源（首次打开需构建资源组）
3. 运行 Assets/Scenes/Main.unity 场景

项目结构

```
Assets/
├── Scripts/          # 核心逻辑（事件中心、拼图AI、难度调节等）
├── Scenes/           # 场景文件
├── AddressableAssets/# 可寻址资源（图片、音频、百科数据）
├── UI/               # 国风UI预制体及图集
├── AI/               # AI Agent 接口与动态难度算法
└── StreamingAssets/  # 外部数据（异兽百科JSON）
```

贡献与许可

本项目为文化+AI创新实践展示，欢迎学术交流与二次开发。
详细技术文档请参阅 Docs/ 目录。
如需商业授权，请联系项目团队。

---

“让每一片拼图，都拼接起千年的想象。”

```

