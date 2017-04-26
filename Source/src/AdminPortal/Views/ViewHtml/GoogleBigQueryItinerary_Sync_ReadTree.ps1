cls
# AddFromOtherRepository.ps1
# from http://stackoverflow.com/questions/23937436/add-subdirectory-of-remote-repo-with-git-subtree
# 2.git read-tree :always overwrite the local subdirectory with the latest version from upstream,. 
$firstTime=$false

#Relative path with trailing slash 
$relativeTargetSubFolder="GoogleBigQueryItinerary" # relative to current directory
$targetSubFolder="Source/src/AdminPortal/Views/ViewHtml/$relativeTargetSubFolder"

$targetBranch="master"
$remoteRepository="https://github.com/Webjet/AnalyticsScripts.git"
$remoteAlias="AnalyticsScripts"
$remoteBranch="master"
$remoteSubfolder="PythonWebAPI/templates"

$aliasFound= & git remote | out-string -stream | select-string $remoteAlias 
if([string]::IsNullOrEmpty($aliasFound))
{
	git remote add -f -t $remoteBranch --no-tags $remoteAlias $remoteRepository
}
if($firstTime)# Do this the first time:
{
	# The next line is optional. Without it, the upstream commits get
	# squashed; with it they will be included in your local history.
	#$ git merge -s ours --no-commit gitgit/master

	# The trailing slash is important in $targetSubFolder!
	$cmd="git read-tree --prefix=$targetSubFolder -u $remoteAlias/$remoteBranch" +':' +$remoteSubfolder
	Write-output $cmd;	invoke-expression $cmd
	#  git read-tree --prefix=$targetSubFolder -u $remoteAlias/$remoteBranch":"$remoteSubfolder
	# git commit
}
else
{
# To overwrite all changes from remote repository as follows:
# The next line is optional. Without it, the upstream commits get
# squashed; with it they will be included in your local history.
# $cmd="git merge -s ours --no-commit $remoteAlias/$remoteBranch"
# Write-output $cmd ; invoke-expression $cmd

 git rm -rf $relativeTargetSubFolder
 git commit $relativeTargetSubFolder
$cmd=" git read-tree --prefix=$targetSubFolder -u $remoteAlias/$remoteBranch" +':' +$remoteSubfolder
 Write-output $cmd ; invoke-expression $cmd
# git commit
# Now fix any conflicts if you'd modified third_party/git-completion.
#$ git commit
}