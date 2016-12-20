#! python
import argparse, json, dodo
import dodo

def RunDODO():
	
        #Parse Arguments!
        parser = argparse.ArgumentParser(description='Runs DODO.')
        parser.add_argument('-ContainerName',help='Name of the DODO containers in deployment template')
        parser.add_argument('-File',help='Fully qualified path to your json template file')
        parser.add_argument('-JSON',help='JSON contents as a string')
        parser.add_argument('-ParametersJSON',help='JSON Parameters contents as a string')
        parser.add_argument('-ParametersFile',help='Fully qualified path to your json parameters file')
        parser.add_argument('--Mode', choices=['file', 'json'], required=True)
        args = parser.parse_args()
        dodo.RunDODO(args.ContainerName , args.File, args.JSON , args.ParametersFile, args.ParametersJSON, args.Mode)

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


RunDODO()