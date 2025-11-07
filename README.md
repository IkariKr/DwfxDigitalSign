# DigitalSign API

一个基于 ASP.NET Core 的 DWFX 文件处理 API，提供水印添加、数字签名、签名验证和签名信息查询功能。

## 功能特性

### 🔒 数字签名
- 支持使用 PFX 证书对 DWFX 文件进行数字签名
- 支持多种哈希算法：SHA1、SHA256、SHA384
- 完整的签名验证功能
- 签名信息查询和解析

### 💧 水印添加
- 支持在 DWFX 文件中添加 PNG 格式水印
- 可自定义水印位置、大小和透明度
- 支持高清晰度水印渲染（1-20 级清晰度）

## 技术栈

- .NET 9+
- ASP.NET Core

## API 端点

### 1. 添加水印
**POST** `/api/watermark`

**请求参数：**
- `DwfxStream` (文件): DWFX 文件流
- `PngStream` (文件): PNG 水印图片流
- `PngWidth` (int): 水印宽度
- `PngHeight` (int): 水印高度
- `PositionX` (float): 水印 X 坐标
- `PositionY` (float): 水印 Y 坐标
- `Clarity` (int): 清晰度级别 (1-20)

**响应：** 包含水印的 DWFX 文件流

### 2. 数字签名
**POST** `/api/digitalsign`

**请求参数：**
- `DwfxStream` (文件): DWFX 文件流
- `PfxStream` (文件): PFX 证书文件流
- `Password` (string): 证书密码
- `HashAlgorithmType` (int): 哈希算法类型 (1:SHA1, 2:SHA256, 3:SHA384)

**响应：** 已签名的 DWFX 文件流

### 3. 验证签名
**POST** `/api/vertifysign`

**请求参数：**
- `file` (文件): 已签名的 DWFX 文件

**响应：** 签名验证结果 (true/false)

### 4. 签名信息
**POST** `/api/signinfo`

**请求参数：**
- `file` (文件): 已签名的 DWFX 文件

**响应：** 签名详细信息

## 安装和运行

### 前提条件
- .NET 9.0 SDK 或更高版本
