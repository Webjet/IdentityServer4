Rem from https://developer.atlassian.com/blog/2015/05/the-power-of-git-subtree
Rem You should create aliases sba and sbu as described in https://developer.atlassian.com/blog/2015/05/the-power-of-git-subtree
Rem If you have errors "Working tree has modifications.  Cannot add." run stash before commang and stash apply after command
Rem git stash 
@rem run this command from the toplevel of the working tree
cd ..
git subtree add --prefix Build/BuildSystem https://github.com/Webjet/BuildScripts.git master --squash
rem git stash apply
@pause
