# 🌌 Unity Huge Scale Rendering Systems

[![Unity](https://img.shields.io/badge/Unity-2022.3.15f1-blue.svg?style=flat&logo=unity)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![WebGL Demo](https://img.shields.io/badge/Demo-WebGL-orange.svg)](https://svlubin.github.io/UnityHugeScaleWebGLBuild/)

This repository demonstrates various rendering techniques for realistic space simulators in Unity, where objects have vast sizes and distances between them exceed the limits of standard floating-point precision.

📖 **Based on the article:** [Система рендеринга для космического симулятора в Unity](https://habr.com/ru/articles/800629/) (Habr).

---

## 🎮 Live Demo

🚀 **Experience it here:** [WebGL Build](https://svlubin.github.io/UnityHugeScaleWebGLBuild/)

> [!TIP]
> By switching between different rendering systems in the demo, you can observe their characteristic issues (shaking, clipping, or Z-fighting) described in the article.

---

## 🛠 Implemented Methods

*   **1️⃣ Simple Scaling Method**: Demonstrates why simple downscaling fails for large-scale scenes due to float precision limits.
*   **2️⃣ Floating Origin Method**: A standard technique that keeps the camera at `(0,0,0)` to maintain precision near the observer.
*   **3️⃣ Cone Scaling Method**: A geometric "hack" that moves and scales objects along the camera's view cone to mimic their appearance.
*   **4️⃣ Multilayer Rendering Method**: The most robust solution that splits the scene into distance layers, rendering each with its own camera.

---

## ⚠️ Important Notes

> [!IMPORTANT]
> **Missing Assets & Setup**
> Prefabs for the space objects (planets, spacecraft, etc.) shown in the article are **not included** due to licensing restrictions of the Unity Asset Store. Generic spheres and dummy objects are used instead. **You will need to manually set up and assign your own models to these dummy objects.**

> [!WARNING]
> **Educational Purpose Only**
> The code is intentionally simplified to focus on demonstrating core ideas. It is **not** a production-ready library. Further optimizations are required for actual project use.

> [!NOTE]
> **Performance**
> While simplified, the multilayer system demonstrates multi-camera management. For production, consider using Unity's **Scriptable Render Pipeline (SRP)** for better control and performance.

---

## 📜 License

This project is provided for educational purposes. See the [LICENSE](LICENSE) file for details.
