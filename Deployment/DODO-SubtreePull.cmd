Rem from https://developer.atlassian.com/blog/2015/05/the-power-of-git-subtree create aliases in ~/.gitconfig
Rem If you have errors "Working tree has modifications.  Cannot add." run stash before commang and stash apply after command
Rem  git stash 
@rem run this command from the toplevel of the working tree
cd ..
git subtree pull --prefix Deployment/DODO https://github.com/Webjet/DODO.git master --squash
Rem git stash apply
@pause
