# 林间灵物信使 - NPC对话系统配置指南

## 📚 系统概述

本对话系统支持以下功能：
- ✅ 打字机效果的文本显示
- ✅ NPC头像和名称显示
- ✅ 对话选项分支
- ✅ 自动触发/手动触发两种模式
- ✅ 视觉交互提示
- ✅ NPC动画状态同步
- ✅ 对话触发区域
- ✅ 可跳过对话

---

## 📁 文件结构

```
Assets/Scripts/
├── Dialogue/
│   ├── DialogueData.cs          # 对话数据配置（ScriptableObject）
│   ├── DialogueManager.cs       # 对话管理器核心
│   └── SampleDialogues.cs       # 示例对话模板
└── NPC/
    ├── NPCInteractable.cs       # NPC交互逻辑
    ├── NPCAnimator.cs           # NPC动画控制
    └── DialogueTriggerZone.cs   # 对话触发区域
```

---

## 🎨 第一步：搭建对话UI界面

### 1.1 创建UI Canvas

1. 在场景中右键 → **UI → Canvas**
2. 设置 Canvas 组件：
   - Render Mode: **Screen Space - Overlay**
   - UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: **1920 x 1080**

### 1.2 创建对话面板结构

```
Canvas
└── DialoguePanel (Image)
    ├── NPCPortrait (Image)
    ├── NPCNameText (Text)
    ├── DialogueText (Text)
    ├── ContinueIndicator (Text/Image)
    └── OptionsContainer (Vertical Layout Group)
```

### 1.3 详细UI配置

#### **DialoguePanel (对话主面板)**
- **组件**: Image
- **颜色**: 深褐色/森林绿 (建议: `#2D1B0E` 或 `#1A3D1A`)
- **透明度**: 200-220
- **锚点**: 底部居中
- **位置**: (0, 100, 0)
- **大小**: (1600, 300)
- **圆角**: 使用圆角Image或添加圆角材质

#### **NPCPortrait (NPC头像)**
- **组件**: Image
- **位置**: (-680, 0, 0)
- **大小**: (180, 180)
- **保留比例**: Enable
- **默认隐藏**: 可以在没有头像时自动隐藏

#### **NPCNameText (NPC名称)**
- **组件**: Text
- **位置**: (-450, 80, 0)
- **字体大小**: 28
- **字体样式**: Bold
- **颜色**: 金黄色 `#FFD700`
- **对齐方式**: 左对齐

#### **DialogueText (对话内容)**
- **组件**: Text
- **位置**: (100, 0, 0)
- **大小**: (900, 200)
- **字体大小**: 24
- **行间距**: 1.2
- **颜色**: 白色/浅米色 `#FFF8E1`
- **对齐方式**: 左上对齐
- **横向/纵向溢出**: Wrap / Overflow

#### **ContinueIndicator (继续提示)**
- **组件**: Text 或 Image
- **文本内容**: "▶" 或 "按空格继续"
- **位置**: (650, -100, 0)
- **字体大小**: 18
- **颜色**: 浅灰色 `#B0B0B0`
- **添加动画**: 闪烁或上下跳动

#### **OptionsContainer (选项容器)**
- **组件**: Vertical Layout Group
- **位置**: (100, -50, 0)
- **大小**: (800, 150)
- **子对象对齐**: Upper Left
- **间距**: 10
- **Padding**: (10, 10, 10, 10)

#### **OptionButtonPrefab (选项按钮预制体)**
创建一个按钮预制体，保存在 `Assets/Prefabs/UI/`：
- **组件**: Button + Image
- **大小**: (700, 50)
- **颜色**: 半透明森林绿
- **子对象**: Text (选项文字，字体大小20)

---

## 🎮 第二步：配置 DialogueManager

### 2.1 创建管理器对象

```
GameObject: DialogueManager
  └── 添加组件: DialogueManager
```

### 2.2 组件参数配置

| 参数 | 说明 | 建议值 |
|------|------|--------|
| Dialogue Panel | 对话面板对象 | 拖拽刚才创建的 DialoguePanel |
| NPC Portrait | NPC头像Image | 拖拽 NPCPortrait |
| NPC Name Text | NPC名称文本 | 拖拽 NPCNameText |
| Dialogue Text | 对话内容文本 | 拖拽 DialogueText |
| Continue Indicator | 继续提示 | 拖拽 ContinueIndicator |
| Options Container | 选项容器 | 拖拽 OptionsContainer |
| Option Button Prefab | 选项按钮预制体 | 拖拽 OptionButtonPrefab |
| Next Dialogue Key | 下一句按键 | KeyCode.Space |

---

## 🦊 第三步：创建森林 NPC

### 3.1 NPC预制体结构

```
NPC_Fox (狐狸)
├── Graphics (精灵渲染)
│   └── SpriteRenderer
├── NPCAnimator (动画控制器)
├── InteractionIndicator (交互提示)
│   └── Sprite (如: "!" 或 "E" 键图标)
└── 以下组件
```

### 3.2 必需组件配置

#### 1. Collider2D (碰撞体)
- 添加 **CircleCollider2D** 或 **BoxCollider2D**
- Is Trigger: **False** (用于物理碰撞)
- 调整大小匹配NPC精灵

#### 2. Rigidbody2D (刚体)
- Body Type: **Kinematic**
- Gravity Scale: **0**

#### 3. NPCInteractable (核心交互组件)

| 参数 | 说明 | 建议值 |
|------|------|--------|
| NPC Id | NPC唯一标识 | "fox_001" |
| Default Dialogue | 默认对话数据 | 后续创建 |
| Can Interact | 是否可交互 | True |
| One Time Only | 是否只触发一次 | False |
| Interaction Radius | 交互触发半径 | 2.0 - 3.0 |
| Player Layer | 玩家层级 | 选择 Player 层 |
| Interaction Key | 手动交互按键 | KeyCode.E |
| Interaction Indicator | 交互提示图标 | 拖拽子对象 |
| Auto Start Dialogue | 自动开始对话 | True |
| Auto Start Delay | 自动触发延迟 | 0.5 |

#### 4. NPCAnimator (动画控制组件)
- 添加 NPCAnimator 组件
- 添加 Animator 组件并配置动画控制器
- 配置参数名称匹配你的Animator参数

### 3.3 NPC 层级设置
- **Layer**: NPC (需要创建新Layer)
- **Tag**: NPC

---

## 📝 第四步：创建对话数据

### 4.1 创建 DialogueData

1. 在 **Project** 窗口右键
2. **Create → Dialogue → Dialogue Data**
3. 命名为 "Dialogue_Fox_Greeting"

### 4.2 对话数据配置示例

#### 示例1：狐狸的问候对话

**NPC信息**:
- NPC Name: **小狐狸**
- NPC Portrait: (导入狐狸头像精灵)

**对话内容**:
```
Line 1: "欢迎来到神秘森林，小旅人！"
Line 2: "我是这片森林的信使，专门负责传递小动物们的信件。"
Line 3: "你是第一次来这里吗？这片森林里藏着很多秘密哦~"
Line 4: "往东边走，你会遇到松鼠弟弟，他好像遇到了一些麻烦。"
```

**对话设置**:
- Typing Speed: **0.05** (值越小越快)
- Can Skip: **True** (允许跳过打字效果)

#### 示例2：带选项的分支对话

**对话内容**:
```
Line 1: "你愿意帮助我寻找丢失的信件吗？"
```

**对话选项**: (勾选 Has Options)
```
选项1: "当然愿意！"
  → Next Dialogue: Dialogue_Fox_QuestAccept
  → Is Exit Option: False

选项2: "我需要先准备一下..."
  → Next Dialogue: (空)
  → Is Exit Option: True
```

---

## 🎯 第五步：对话触发区域

### 5.1 创建触发区域

```
GameObject: DialogueTrigger_Entrance
  ├── 添加组件: DialogueTriggerZone
  └── 添加组件: BoxCollider2D (或 CircleCollider2D)
```

### 5.2 组件配置

- Collider2D → Is Trigger: **True**
- Dialogue To Trigger: 选择要触发的对话数据
- Trigger Once: **True** (只触发一次)
- Disable After Trigger: **True**
- Player Tag: "Player"

---

## 🎬 第六步：示例对话内容

### 森林小动物对话模板

#### 🦊 小狐狸（引导NPC）
```
"欢迎来到神秘森林！"
"我是这里的信使，负责传递所有小动物的信件。"
"最近森林里出现了一些奇怪的现象，很多信件都迷路了..."
"如果你能帮我找回这些信件，小动物们一定会很感谢你的！"
```

#### 🐿️ 小松鼠（任务NPC）
```
"呜呜呜...你好！"
"我藏起来的松果信件不见了！那是给猫头鹰爷爷的生日礼物..."
"我记得好像是往西边的小溪那边去了，你能帮我找找吗？"
"找到的话，我给你我收藏的金色橡子！"
```

#### 🐰 小兔子（信息NPC）
```
"嘘...小声一点~"
"你知道吗？森林深处住着一位很老很老的乌龟爷爷。"
"据说他知道这片森林所有的秘密..."
"不过他住在蘑菇洞穴里，很少出来见人。"
```

---

## ⚙️ 第七步：玩家配置更新

### PlayerController 交互集成

确保玩家对象配置正确：

```
Player 对象
  ├── Layer: Player (重要！)
  ├── Tag: Player (重要！)
  ├── Collider2D (已存在)
  └── Rigidbody2D (已存在)
```

---

## 🎨 第八步：视觉效果建议

### 8.1 交互提示图标
- 使用 "!" 图标或 "E" 键图标
- 添加上下浮动动画
- 玩家进入范围时显示，离开时隐藏
- 颜色：黄色/金色，带有轻微发光效果

### 8.2 NPC对话时的视觉反馈
- NPC播放说话动画
- 对话面板淡入淡出
- 文字颜色渐变
- 添加打字音效

### 8.3 对话选项视觉
- 选中高亮
- 悬停效果
- 点击反馈动画

---

## 🐛 常见问题排查

### Q: 对话不触发？
1. 检查 Player 的 Layer 和 Tag 是否正确
2. 检查 NPCInteractable 的 Player Layer 是否选中了 Player 层
3. 检查 Interaction Radius 大小是否合适
4. 查看 Console 是否有错误信息

### Q: UI 不显示？
1. 检查 Canvas 的 Render Mode 设置
2. 确认 UI 元素的锚点和位置正确
3. 检查 DialogueManager 的引用是否都已赋值

### Q: 打字机效果不工作？
1. 确认 Coroutine 能正常运行
2. 检查 Typing Speed 值是否合理 (0.02-0.1)
3. 检查 Text 组件的大小是否足够显示文本

### Q: NPC 动画不切换？
1. 确认 Animator Controller 中参数名称正确
2. 检查 NPCAnimator 组件的参数名称配置
3. 确认事件订阅正确

---

## 🔧 高级功能扩展

### 扩展1：对话音效
```csharp
// 在 DialogueManager 的 TypeText 协程中添加
if (typingSound != null && charCount % 3 == 0)
{
    AudioSource.PlayClipAtPoint(typingSound, transform.position);
}
```

### 扩展2：对话条件系统
```csharp
// 添加对话前置条件检查
public bool CheckDialogueConditions()
{
    // 检查任务进度、物品拥有等条件
    return QuestManager.Instance.HasActiveQuest("FindLetters");
}
```

### 扩展3：对话存档
```csharp
// 记录已完成的对话
public void SaveDialogueProgress()
{
    PlayerPrefs.SetInt($"Dialogue_{npcId}_Completed", 1);
}
```

---

## ✅ 系统检查清单

- [ ] 对话 UI 面板创建完成
- [ ] DialogueManager 配置完成
- [ ] NPC 预制体创建并配置组件
- [ ] 对话数据 ScriptableObject 创建
- [ ] 对话数据内容填写
- [ ] Player 层级和标签设置正确
- [ ] 交互半径测试通过
- [ ] 自动触发/手动触发功能测试
- [ ] 对话选项分支测试
- [ ] NPC 动画状态同步测试

恭喜！你的森林NPC对话系统已经搭建完成！🌲🦊✨
