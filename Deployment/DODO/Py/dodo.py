#! python
import argparse, json
from dodo_dcos_mesos import dodo_dcos_mesos

def RunDODO( ContainerName , File, JSON , ParametersFile, ParametersJSON, Mode):
	print("     ______ __")
	print("   {-_-_= '. `'.")
	print("    {=_=_-  \   \\")
	print("     {_-_   |   /")
	print("      '-.   |  /    .===,")
	print("   .--.__\  |_(_,==`  ( o)'-.")
	print("  `---.=_ `     ;      `/    \\")
	print("      `,-_       ;    .'--') /")
	print("        {=_       ;=~  `  `\"")
	print("         `//__,-=~")
	print("         <<__ \\__")
	print("         /`)))/`)))")

        jsonTemplate = ''
        jsonParameters = ''
        #Validate Arguments!
        if(Mode == None):
            raise Exception("--Mode is required. See usage.")
        elif(Mode == 'file'):
            print("Processing file --> " + File)
            with open(File) as txt:
                jsonTemplate = json.load(txt)
            if(ParametersFile != None and ParametersFile != ''):
                with open(ParametersFile) as paramTxt:
                    jsonParameters = json.load(paramTxt)
        elif(Mode == 'json'):
            print(JSON)
            jsonTemplate = json.loads(JSON)
            if(ParametersJSON != None and ParametersJSON != ''):
                jsonParameters = json.loads(ParametersJSON)      
        
        if(jsonParameters is None or  jsonParameters is ''):
            print("No Parameter file or json could be parsed!")

        if(jsonTemplate is None or  jsonTemplate is ''):
            raise Exception("Your deployment template could not be parsed!")
        
        #Parameter injection
        jsonTemplate = __SetInternalDODOParameters(jsonTemplate, jsonParameters);

        #Variable injection
        jsonTemplate = __SetInternalDODOVariables(jsonTemplate);
        
        #Container checks
        if("Containers" in jsonTemplate):
            containers = jsonTemplate["Containers"]

            if(ContainerName == None):
                for container in containers:
                    __ProcessContainer(container)
            else:
                for container in containers:
                    if(container["Name"] == ContainerName):
                        __ProcessContainer(container)
        else:
            print("Configuration container is blank and not specified!")        
               
        return;

def __ProcessContainer(container):
    type = container["Type"]
    name = container["Name"]
    print("Processing DODO container : " + name)
    if(type == 'DCOSMesosService'):
        worker = dodo_dcos_mesos(container)
        worker.Deploy_DODODCOSMesosService()

    return; 

def __SetInternalDODOVariables( jsonTemplate ):
    print("Executing SetInternalDODOVariables")

    if("Variables" in jsonTemplate):
        jsonRaw = json.dumps(jsonTemplate)
        variables = jsonTemplate["Variables"]
        for variable in variables:
            print("key: " + variable + "    value: " + variables[variable])
            jsonRaw = jsonRaw.replace("[variables('" + variable + "')]", variables[variable])
        
        print("Variables injected!")
        jsonTemplate = json.loads(jsonRaw)
        
    else:
        print("No Variables specified, moving on!")
        
    print("Done executing  SetInternalDODOVariables")
    return jsonTemplate


def __SetInternalDODOParameters (jsonTemplate , jsonParameters):
    print("Executing SetInternalDODOParameters")

    if("Parameters" in jsonParameters):
       
        parameters = jsonParameters["Parameters"]
        for parameter in parameters:
            print("param : " + parameter + "    value: " + parameters[parameter])
            (jsonTemplate["Variables"])[parameter] = parameters[parameter]
        print("Parameter injected!")
      
        
    else:
        print("No Parameters specified, moving on!")
       
    print("Done executing  SetInternalDODOParameters")
    return jsonTemplate;