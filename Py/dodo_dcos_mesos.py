#! python
import urllib , json, codecs, httplib, urllib2, time

class dodo_dcos_mesos(object):

    def __init__(self, container):
        self.container = container
        return;
    def Deploy_DODODCOSMesosService(self):
        print(self.container)
        #initialise variables from json
        self.appName = ((self.container["Attributes"])["Properties"])["ApplicationName"]
        self.marathonServer = ((self.container["Attributes"])["Properties"])["MarathonServer"]
        self.marathonPort = ((self.container["Attributes"])["Properties"])["MarathonPort"]
        self.deploymentID = ((self.container["Attributes"])["Properties"])["ServiceDeploymentIdentifier"]
        self.marathonTemplate = (self.container["Attributes"])["MarathonTemplate"]
        self.__Deploy();
        self.__UpdateLoadBalancer(self.appName, "/" + self.appName + "/" + self.deploymentID);
        self.__DestroyPreviousVersions(self.appName, "/" + self.appName + "/" + self.deploymentID);
        return;
    def __Deploy(self):
        reader = codecs.getreader("utf-8")
        print("Starting deployment - Parsing template...")
       
        deployment = json.dumps(self.marathonTemplate).replace("$ID", self.deploymentID)
        print("Starting deployment - Template parsed successfully!")
        print(deployment)

        print("Starting deployment - Deploying...")
        headers = {"Content-type": "application/json"}
        conn = httplib.HTTPConnection(self.marathonServer, int(self.marathonPort))
        conn.connect()
        conn.request("POST", "/marathon/v2/apps", body=deployment, headers=headers)
        response = conn.getresponse()
        print(response.status, response.reason)

        if(response.status == 200 or response.status == 201):
            print("Starting deployment - Success!")
            self.__MakeSureItsRunning( "/" + self.appName + "/" + self.deploymentID )
        else:
            raise Exception("Deployment failed, check reason!")
        return;

    def __MakeSureItsRunning(self,  applicationID ):
        busy = True

        while busy:
            reader = codecs.getreader("utf-8")
            print("Checking application tasks...")
            tasks = json.load(reader((urllib2.urlopen(urllib2.Request("http://" + self.marathonServer + "/marathon/v2/apps/" + applicationID + "/tasks")))))
            print(tasks)
            taskCount = len(tasks.keys())
            tasksRunning = 0
            
            for task in tasks["tasks"]: 
                print("Task state = " + task["state"])
                if(task["state"] == "TASK_RUNNING"):
                    if("healthCheckResults" in task):
                        healthCheckCount = len(task["healthCheckResults"])
                        healthCheckCounter = 0
                        for healthCheck in task["healthCheckResults"]:
                            if("alive" in healthCheck and healthCheck["alive"] == True):
                                healthCheckCounter += 1
                        
                        if(healthCheckCounter == healthCheckCount):
                            tasksRunning += 1
                                
            if(taskCount == tasksRunning):
                print("All tasks are running and healthy!")
                busy = False
            else:
                print("Not all tasks are running\healthy!")
                busy = True
            
            time.sleep(1);
        return;

    def __UpdateLoadBalancer(self, applicationName, applicationID):
        reader = codecs.getreader("utf-8")
        print("Checking application deployments...")
        apps = json.load(reader((urllib2.urlopen(urllib2.Request("http://" + self.marathonServer + "/marathon/v2/apps?id=" + applicationName)))))
    
        for app in apps["apps"]:
            if(app["id"] == applicationID):
                print("latest app :" + app["id"])
            else:
                print("Detected previous version: " + app["id"])
                print("Update - Removing " +  app["id"] + " from load balancer...")
                headers = {"Content-type": "application/json"}
                conn = httplib.HTTPConnection(self.marathonServer,int(self.marathonPort))
                conn.connect()
                deployment = '{ "id": "' + app["id"] + '" , "labels": { "HAPROXY_0_VHOST": "" } }'
                print(deployment)             
                conn.request("PUT", "/marathon/v2/apps" + app["id"], body=deployment, headers=headers)
                response = conn.getresponse()
                print(response.status, response.reason)

                if(response.status == 200):
                    print("Update - Success!")
                    self.__MakeSureItsRunning( app["id"]);
                    print("Waiting for load balancer...")
                    time.sleep(20)
                    print("Done")
                else:
                    raise Exception("Deployment failed, check reason!")
        return;

    def __DestroyPreviousVersions(self, applicationName, applicationID):
        reader = codecs.getreader("utf-8")
        print("Checking application deployments...")
        apps = json.load(reader((urllib2.urlopen(urllib2.Request("http://" + self.marathonServer + "/marathon/v2/apps?id=" + applicationName)))))
    
        for app in apps["apps"]:
            if(app["id"] == applicationID):
                print("latest app :" + app["id"])
            else:
                print("Detected previous version: " + app["id"])
                print("Destroying " +  app["id"] + "...")
                headers = {"Content-type": "application/json"}
                conn = httplib.HTTPConnection(self.marathonServer,int(self.marathonPort))
                conn.connect()
                conn.request("DELETE", "/marathon/v2/apps" + app["id"], "", headers)
                response = conn.getresponse()
                print(response.status, response.reason)

                if(response.status == 200):
                    print("Delete - Success!")
                else:
                    raise Exception("Deployment failed, check reason!")
        return;



