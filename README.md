# Arabidopsis Citation

<p align="center">
  <img src="Docs/img/logo.png" width="200" alt="Arabidopsis Citation Logo">
  <br>
  <strong>科研文献管理与实验规划一体化解决方案</strong>
</p>

## 🌟 项目简介

Arabidopsis Citation 是一款专为科研工作者设计的开源文献管理工具，基于WPF框架开发。它整合了文献引用管理、实验任务规划和科研数据整理等功能，帮助研究者高效组织科研工作流。

## ✨ 核心功能

### 📚 文献管理
- 智能DOI识别与元数据抓取
- 支持APA/MLA/Chicago等学术引用格式
- RIS格式批量导入导出
- 按课题分类管理文献

### ⏰ 任务规划
- 可视化实验时间线
- 定时提醒功能
- 任务进度追踪
- 团队协作支持

### 🔒 数据安全
- 项目文件加密保护
- 自动备份功能
- 多设备数据同步

## 🛠 技术架构

• 前端: WPF (Windows Presentation Foundation)
• 框架: .NET 9
• 数据库: Microsoft Access
• 构建工具: dotnet

## 🚀 快速开始

```powershell
# 1. 安装.NET 9 SDK
winget install Microsoft.DotNet.SDK.9

# 2. 克隆仓库
git clone https://github.com/ArabidopsisDev/Citation.git
cd citation

# 3. 还原NuGet包
dotnet restore

# 4. Release模式编译
dotnet build -c Release

# 5. 是我喜欢的程序，直接运行
.\bin\Debug\net9.0-windows\Citation.exe
```

或者也可以直接在发布版中下载最新版。

## 📖 文档资源

| 文档类型 | 链接 |
|----------|------|
| 中文用户手册 | [Docs/manual-cn.md](Docs/manual-cn.md) |
| English Manual | [Docs/manual-en.md](Docs/manual-en.md) |

## 🤝 参与贡献

```plaintext
1. Fork项目仓库
2. 创建特性分支 (git checkout -b feature/your-feature)
3. 提交修改 (git commit -m 'Add your feature')
4. 推送分支 (git push origin feature/your-feature)
5. 创建Pull Request
```

## 📬 联系我们

```plaintext
✉️ 邮箱: arab@methodbox.top
👥 QQ群: 1053379975
```

---

<p align="center">
  <sub>📜 AGPL License | 🏷️ Version alpha 0.2</sub>
</p>