Rem from https://developer.atlassian.com/blog/2015/05/the-power-of-git-subtree
Rem You should create aliases sba and sbu as described in https://developer.atlassian.com/blog/2015/05/the-power-of-git-subtree
Rem If you have errors "Working tree has modifications.  Cannot add." run stash before commang and stash apply after command
git stash 
@rem git sba https://github.com/Webjet/DODO.git Deployment/DODO
@rem run this command from the toplevel of the working tree
cd ..
git subtree add --prefix Deployment/DODO https://github.com/Webjet/DODO.git master --squash
git stash apply
@pause
