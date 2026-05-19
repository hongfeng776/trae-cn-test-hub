# Unity 4x4 方格游戏

这是一个Unity项目，实现了4x4方格界面，包含3种不同图标预制体的随机填充功能，以及点击选中和高亮功能。

## 项目结构

```
Assets/
├── Scripts/
│   ├── GridManager.cs      # 网格管理器，负责生成、填充网格和选择逻辑
│   ├── Cell.cs             # 单元格脚本（含点击和高亮功能）
│   ├── IconType.cs         # 图标类型定义和图标脚本
│   └── GameManager.cs       # 游戏管理器
├── Prefabs/
│   ├── Cell.prefab          # 单元格预制体
│   ├── Icon_Type1.prefab     # 图标1（红色圆形）
│   ├── Icon_Type2.prefab     # 图标2（绿色方形）
│   └── Icon_Type3.prefab     # 图标3（蓝色菱形）
└── Scenes/
    └── MainScene.unity       # 主场景
```

## 功能特性

### 基础功能
- **4x4方格网格**：自动生成4行4列的方格布局
- **三种图标类型**：
  - Type1：红色圆形
  - Type2：绿色方形
  - Type3：蓝色菱形
- **随机填充**：游戏启动时自动随机填充图标
- **刷新功能**：可通过按钮重新随机填充图标

### 新增：点击选中功能 ✨
- **点击选中**：点击带图标的单元格可以选中
- **高亮效果**：
  - 选中状态：金色背景 + 图标放大20%
  - 相邻高亮：选中第一个图标后，其上下左右相邻的可用图标会高亮显示（浅黄色）
- **相邻选择**：只能选择两个相邻的图标（上下左右方向）
- **智能取消**：
  - 再次点击已选中的图标取消选择
  - 点击非相邻图标切换选择目标
  - 选中两个后再次点击自动重置

### ✅ 修复内容（最新更新）
- **空引用保护**：所有方法添加完善的null检查，防止报错
- **预制体验证**：启动时自动检查配置，缺少组件时给出明确提示
- **相邻判断加强**：`IsValidAndAdjacent()` 确保只有相邻且有图标才能选择
- **相同图标判定**：多层空值检查确保类型比较安全
- **Debug日志**：匹配成功/失败时输出清晰的日志信息
- **内存管理**：添加 `OnDestroy()` 正确取消事件订阅

## 如何使用步骤

### 1. 在Unity中打开项目

1. 打开Unity Hub
2. 点击"Add"按钮，选择此项目文件夹
3. 等待Unity导入项目

### 2. 设置场景

1. 打开 `Scenes/MainScene`
2. 在Hierarchy中创建以下对象：

#### 创建Canvas
```
- Canvas (设置为Screen Space - Overlay)
  ├── GridPanel (添加GridLayoutGroup组件)
  │   └── (GridManager会在这里动态生成Cell)
  └── RefreshButton (UI Button)
```

#### 配置GridManager

1. 在GridPanel对象上添加GridManager脚本
2. 在Inspector中设置：
   - Grid Size: 4
   - Cell Prefab: 拖拽 Prefabs/Cell
   - Icon Prefabs:  添加3个元素，分别拖拽3种图标预制体
   - Grid Parent: 拖拽GridPanel自身
   - Cell Spacing: 10

#### 配置GameManager

1. 在场景中创建空对象，命名为GameManager
2. 添加GameManager脚本
3. 设置：
   - Grid Manager: 拖拽GridPanel对象
   - Refresh Button: 拖拽RefreshButton对象

### 3. 运行游戏

点击Play按钮运行：
- 4x4的方格网格自动生成
- 每个格子随机显示一种颜色的图标
- 左上角实时显示当前得分
- **点击任意图标**：选中该图标，相邻可点击的图标会高亮
- **点击相邻高亮图标**：选中第二个图标
  - ✅ **图标相同** → 两个图标消除，**得分+10**
  - 消除后**上方图标自动下落**填补空位
  - 顶部**自动生成新图标**补位
  - ❌ **图标不同** → 无反应，自动取消选中
- **点击刷新按钮**：重新随机填充所有图标，分数归零

## 脚本说明

### GridManager.cs
- `InitializeGrid()`: 初始化4x4网格，创建所有单元格并绑定点击事件
- `RandomFillIcons()`: 随机填充图标到每个单元格
- `RefreshGrid()`: 重新随机刷新所有图标
- `HandleCellClicked()`: 处理单元格点击事件
- `SelectFirstCell()`: 选中第一个单元格并高亮相邻格子
- `SelectSecondCell()`: 选中第二个相邻单元格
- `IsAdjacent()`: 判断两个单元格是否相邻
- `HighlightAdjacentCells()`: 高亮相邻可用单元格
- `ClearAllSelections()`: 清除所有选中和高亮状态

**事件回调**：
- `OnTwoCellsSelected`: 当成功选中两个相邻单元格时触发

### Cell.cs
- 管理单个单元格的状态
- 实现 `IPointerClickHandler` 接口处理点击
- **状态管理**：
  - `SetSelected(bool)`: 设置选中状态
  - `SetHighlighted(bool)`: 设置高亮状态
- **视觉效果**：
  - 正常状态：灰色背景
  - 选中状态：金色背景 + 图标放大20%
  - 高亮状态：浅黄色半透明背景
- **事件回调**：
  - `OnCellClicked`: 单元格被点击时触发

### IconType.cs
- 定义三种图标类型枚举
- Icon脚本存储图标类型和颜色信息

### GameManager.cs
- 单例模式管理游戏
- 处理刷新按钮点击事件

## 可自定义配置

### 选择效果配置（Cell.cs Inspector）
- **Normal Color**: 正常状态背景色（默认：灰色）
- **Selected Color**: 选中状态背景色（默认：金色）
- **Highlight Color**: 高亮状态背景色（默认：浅黄色半透明）

### 网格配置（GridManager.cs Inspector）
- **Grid Size**: 网格大小（默认：4）
- **Cell Spacing**: 单元格间距（默认：10）

### 其他扩展
- **修改网格大小**：在GridManager中修改Grid Size参数
- **添加新图标类型**：
  1. 在IconType.cs中添加新枚举值
  2. 创建新的图标预制体
  3. 在GridManager的Icon Prefabs数组中添加新预制体
- **修改单元格大小**：修改Cell预制体的SizeDelta或GridLayoutGroup的Cell Size
- **修改图标缩放**：在Cell.cs的UpdateVisualState()中调整缩放系数

## 扩展开发：处理两个选中的图标

订阅 `GridManager.OnTwoCellsSelected` 事件来处理两个选中的图标：

```csharp
void Start()
{
    GridManager gridManager = FindObjectOfType<GridManager>();
    gridManager.OnTwoCellsSelected += OnTwoIconsSelected;
}

void OnTwoIconsSelected(Cell cell1, Cell cell2)
{
    // 获取两个图标的信息
    Icon icon1 = cell1.currentIcon;
    Icon icon2 = cell2.currentIcon;
    
    // 可以在这里实现：
    // - 交换图标
    // - 消除相同图标
    // - 播放动画
    // - 等等...
    
    Debug.Log($"选中了两个图标：{icon1.iconType} 和 {icon2.iconType}");
}
```

## 技术要求

- Unity 2020.3 或更高版本
- .NET Framework 4.x
- Unity UI 包（内置）
