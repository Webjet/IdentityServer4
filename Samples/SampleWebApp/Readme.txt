

Ensure Web Project (.csproj) has the following block of XML under its Release mode <PropertyGroup> node:

<PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">

  <!--ADD: -->
    <WebPublishMethod>Package</WebPublishMethod>
    <DeployOnBuild>true</DeployOnBuild>
    <DeployTarget>Package</DeployTarget>
    <DisableAllVSGeneratedMSDeployParameter>True</DisableAllVSGeneratedMSDeployParameter>
    <PackageAsSingleFile>true</PackageAsSingleFile>
    <FilesToIncludeForPublish>OnlyFilesToRunTheApp</FilesToIncludeForPublish>
</PropertyGroup>


If you dont have this you wont get a web deploy package output!