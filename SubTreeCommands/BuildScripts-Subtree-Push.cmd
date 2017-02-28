Rem from https://medium.com/@v/git-subtrees-a-tutorial-6ff568381844#.a2ne9vlve
Rem git subtree push
@rem run this command from the toplevel of the working tree
cd ..\..\.. 
git subtree push --prefix=Build/BuildScripts https://github.com/Webjet/BuildScripts.git master
@pause