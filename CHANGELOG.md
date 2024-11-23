# Changelog

All notable changes to this project will be documented in this file.

This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

<!-- EasyBuild: START -->
<!-- last_commit_released: ca7a733951f6c728beb7d177637f187f843d57a5 -->
<!-- EasyBuild: END -->

## 2.1.1

### 🐞 Bug Fixes

- Make the footer parser work when body has trailing new lines ([ca7a733](https://github.com/easybuild-org/EasyBuild.CommitParser/commit/ca7a733951f6c728beb7d177637f187f843d57a5))

## 2.1.0

### 🚀 Features

- Add `DotNet.ReproducibleBuilds` trying to make symbols available for debugging purpose by consumer code ([890cb3b](https://github.com/easybuild-org/EasyBuild.CommitParser/commit/890cb3b73046e4ff38101b05e04d2647a0ee9908))

## 2.0.0

### 🚀 Features

- Make the implementation aligned with Conventional Commits specification ([9c97eb5](https://github.com/easybuild-org/EasyBuild.CommitParser/commit/9c97eb5912fced651f37a1c45bda488e48b047fd))

## 1.2.0

### 🚀 Features

- Add `Body` to `CommitMessage` ([eea6c0a](https://github.com/easybuild-org/EasyBuild.CommitParser/commit/eea6c0a2f69fe8655b6afc26810e86eeff12f49d))
- Improve error message if tag is missing and we have a list of tags in the config file ([98ffdf6](https://github.com/easybuild-org/EasyBuild.CommitParser/commit/98ffdf66afa113be7a7396d1f770061be74de63c))
- Re-add `FsToolkit.ErrorHandling` to make the code easier to read (this also bump minimal version of FSharp.Core) ([1f5b8de](https://github.com/easybuild-org/EasyBuild.CommitParser/commit/1f5b8deb7d10dcab100162c8719b39ce7462cf08))

## 1.1.1

### 🐞 Bug Fixes

- Scope and breaking changes are now detected correctly ([c8f1d18](https://github.com/easybuild-org/EasyBuild.CommitParser/commit/c8f1d18318aa98a2a8a1a55468ef879ff194c65f))

## 1.1.0

### 🚀 Features

- Lower FSharp.Core requirements ([1d17830](https://github.com/easybuild-org/EasyBuild.CommitParser/commit/1d17830b97542540e96489f89544ccc7605bd7c9))

## 1.0.0

### 🚀 Features

- Rework the API to be more like a library instead of an internal package ([4c6d03b](https://github.com/easybuild-org/EasyBuild.CommitParser/commit/4c6d03bb543b408fe3c74673a4ed55989fe177fa))

## 0.1.0

### 🚀 Features

- Initial implementation ([2ae7b98](https://github.com/easybuild-org/EasyBuild.CommitParser/commit/2ae7b988d676b84f3432fe14a4b701d963081128))
