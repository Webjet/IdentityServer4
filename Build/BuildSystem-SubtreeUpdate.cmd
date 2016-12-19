Rem from https://developer.atlassian.com/blog/2015/05/the-power-of-git-subtree create aliases in ~/.gitconfig
Rem You should create aliases sba and sbu as described in https://developer.atlassian.com/blog/2015/05/the-power-of-git-subtree
Rem If you have errors "Working tree has modifications.  Cannot add." run stash before commang and stash apply after command
Rem  git stash 
git sbu https://github.com/Webjet/PsakeBuildScript.git Build/BuildSystem
Rem git stash apply
@pause
