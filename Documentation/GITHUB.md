# GitHub: clone and update

Repository: https://github.com/JoshuaWX/Team5-qPCR-Unity

The original asset rights are unverified. The owner has requested publication and plans to change visibility. Make it private in GitHub **Settings → General → Danger Zone → Change repository visibility** before inviting wider access. A visibility change cannot recall copies already downloaded.

## First checkout

Install Git LFS, then:

```powershell
git lfs install
git clone https://github.com/JoshuaWX/Team5-qPCR-Unity.git
cd Team5-qPCR-Unity
git lfs pull
git lfs fsck
```

Add this folder to Unity Hub using Unity 6000.3.25f1. Open `Assets/Team5/Scenes/Team5_qPCR_RealisticLab.unity`, not SampleScene. Allow packages/imports to finish before Play.

## Future updates

Save scenes and assets in Unity, then review before committing:

```powershell
git status
git diff --stat
git add Assets Packages ProjectSettings ArtSource Documentation README.md .gitignore .gitattributes
git diff --cached --stat
git commit -m "Describe the change"
git push
```

Keep `.meta` files: they preserve Unity references. Do not commit Library, Temp, Logs, build outputs, caches, personal credentials or signing keys. Existing `.gitattributes` sends FBX, Blender sources, textures and fonts through Git LFS. Do not use force push to solve an ordinary upload error. Unity being linked to GitHub does not automatically stage, commit or push files.
