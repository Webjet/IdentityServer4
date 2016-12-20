#! python
import  json, sys, os
#sys.path.append('C:\Git\DODO\Py')
import dodo

#Load parameter file
template = ''
with open("C:\Git\DODO\Samples\Templates\dodosample-dcos-mesos-service.json") as txt:
    template = txt.read()

parameters = '{ "Parameters": { "ServiceDeploymentIdentifier" : "test-2" } }'

dodo.RunDODO("Lookup Application Master" , None, template, None, parameters, "json")
#dodo.RunDODO("Lookup Application Develop" , None, template, parameters, None, "json")

