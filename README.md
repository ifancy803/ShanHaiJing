# Mythical Beasts Puzzle Album

*An AI-Enhanced Cultural Learning Experience Inspired by The Classic of Mountains and Seas*

## Overview

Mythical Beasts Puzzle Album is a Unity-based interactive experience that introduces the cultural world of *The Classic of Mountains and Seas* through jigsaw puzzles. It includes a knowledge base covering nine mythical creatures and combines puzzle gameplay, cultural interpretation, and AI-assisted content creation.

The project brings together Unity and C#, Addressable asset management, an event-hub architecture, AI-generated art and music, behavior-based difficulty adjustment, and a conversational cultural assistant. Its goal is to turn classical descriptions into an experience that players can explore, learn from, and collect.

## Key Features

- **Adaptive jigsaw puzzles:** 9-, 16-, and 25-piece modes, drag-and-snap interactions, and automatic completion detection. Difficulty can adapt to player completion time and error rate.
- **Nine-creature knowledge base:** Completing puzzles unlocks illustrations and reference cards with each creature's name, source, appearance, and legends. Selecting illustrated areas can reveal additional notes.
- **Conversational cultural assistant:** Players can ask natural-language questions about creatures and their stories.
- **AI-assisted production workflow:** AI tools support art, music, and code development.
- **Chinese-inspired visual direction:** Ink-wash imagery, mythical-fantasy art, paper textures, and bronze-inspired colors shape the visual identity.

## Technology

- Unity and C#
- Addressable Asset System
- Generic event hub for decoupled communication
- AI-assisted art, music, and code workflows
- Behavior-based adaptive difficulty
- Conversational cultural assistant

## Requirements

- Unity 2021.3 LTS or later
- The repository describes support for Windows, macOS, iOS, and Android; platform-specific builds may require additional configuration.

## Getting Started

1. Clone this repository.
2. Open the project root in Unity Hub with a compatible Unity version.
3. If the project uses Addressables in the checked-out revision, configure and build the Addressable content groups.
4. Open the project's main scene from `Assets/Scenes/` and press **Play**.

> **Repository-specific setup:** Verify the exact main scene and repository URL before publishing this README. The original README contained a placeholder clone URL and named `Assets/Scenes/Main.unity`; confirm that this scene exists in the current repository revision.

## Project Structure

The original project documentation describes the following organization. Adjust paths to match the current repository if they have changed.

```text
Assets/
├── Scripts/           # Core gameplay, event hub, puzzle logic, and difficulty adjustment
├── Scenes/            # Unity scenes
├── AddressableAssets/ # Addressable images, audio, and reference data
├── UI/                # UI prefabs and atlases
├── AI/                # Agent integration and adaptive-difficulty logic
└── StreamingAssets/   # External data such as creature reference JSON
```

## 中文说明

# 《异兽拼图册》

*一款以《山海经》为灵感、由 AI 全链路赋能的文化认知体验*

## 项目简介

《异兽拼图册》是一款基于 Unity 开发的交互体验，通过拼图玩法带领玩家认识《山海经》的文化世界。项目内置涵盖九种神话异兽的知识库，将拼图玩法、文化解读与 AI 辅助内容创作结合起来。

项目整合 Unity 与 C#、Addressable 资源管理、事件中心架构、AI 生成美术与音乐、基于玩家行为的难度调节，以及对话式文化助手，尝试把古代典籍中的描述转化为可探索、可学习、可收集的体验。

## 核心功能

- **自适应拼图：**提供 9、16、25 块拼图模式，支持拖拽吸附与自动完成检测。难度可根据玩家完成时间和失误率调整。
- **九种异兽知识库：**完成拼图后可解锁插画与资料卡，了解异兽名称、出处、形态和传说；点击插画中的部位可查看补充说明。
- **对话式文化助手：**玩家可以通过自然语言询问异兽及其故事。
- **AI 辅助创作流程：**使用 AI 工具辅助美术、音乐与代码开发。
- **东方美术风格：**融合水墨与神话幻想风格，并以宣纸肌理和古铜色调塑造视觉体验。

## 技术栈

- Unity 与 C#
- Addressable Asset System
- 用于解耦通信的泛型事件中心
- AI 辅助美术、音乐与代码工作流
- 基于玩家行为的自适应难度
- 对话式文化助手

## 环境要求

- Unity 2021.3 LTS 或更高版本
- 原项目文档列出 Windows、macOS、iOS 和 Android 支持；各平台构建可能需要额外配置。

## 开始使用

1. 克隆本仓库。
2. 使用兼容版本的 Unity Hub 打开项目根目录。
3. 如果当前版本使用 Addressables，请配置并构建 Addressable 内容组。
4. 在 `Assets/Scenes/` 中打开项目主场景并点击 **Play**。

> **发布前请核实：**原 README 使用了占位克隆地址，并指定 `Assets/Scenes/Main.unity`；请确认当前仓库中该场景和路径确实存在后再发布此说明。

## 项目结构

以下结构来自原项目说明。如仓库目录已有变化，请相应更新。

```text
Assets/
├── Scripts/           # 核心玩法、事件中心、拼图逻辑与难度调节
├── Scenes/            # Unity 场景
├── AddressableAssets/ # Addressable 图片、音频与资料数据
├── UI/                # UI 预制体与图集
├── AI/                # Agent 接入与动态难度逻辑
└── StreamingAssets/   # 异兽资料 JSON 等外部数据
```
