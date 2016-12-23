#
# AddFromOtherRepository.ps1
# from http://stackoverflow.com/questions/23937436/add-subdirectory-of-remote-repo-with-git-subtree
#Relative path with trailing slash 
$targetSubFolder="Source/AdminPortal/Views/GoogleBigQueryItinerary/Content/"
$targetBranch="master"
$remoteRepository="https://github.com/Webjet/AnalyticsScripts.git"
$remoteAlias="AnalyticsScripts"
$remoteBranch="master"
$remoteSubfolder="PythonWebAPI/templates"
if($firstTime)# Do this the first time:
{
git remote add -f -t $remoteBranch --no-tags $remoteAlias $remoteRepository
# The next line is optional. Without it, the upstream commits get
# squashed; with it they will be included in your local history.
#$ git merge -s ours --no-commit gitgit/master
}
# The trailing slash is important here!
$cmd="git read-tree --prefix=$targetSubFolder -u $remoteAlias/$remoteBranch" +':' +$remoteSubfolder
Write-output $cmd
invoke-expression $cmd
#  git read-tree --prefix=$targetSubFolder -u $remoteAlias/$remoteBranch":"$remoteSubfolder
# git commit

# In future, you can merge in additional changes as follows:
# The next line is optional. Without it, the upstream commits get
# squashed; with it they will be included in your local history.
$ git merge -s ours --no-commit $remoteAlias/$remoteBranch
# Replace the SHA1 below with the commit hash that you most recently
# merged in using this technique (i.e. the most recent commit on
# gitgit/master at the time).
#$ git diff --color=never 53e53c7c81ce2c7c4cd45f95bc095b274cb28b76:contrib/completion gitgit/master:contrib/completion | git apply -3 --directory=third_party/git-completion
# Now fix any conflicts if you'd modified third_party/git-completion.
#$ git commit