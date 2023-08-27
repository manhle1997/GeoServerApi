using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace GeoServerApi.GeoServer
{
    public class RestClient
    {
        public String METHOD_DELETE = "DELETE";
        public String METHOD_GET = "GET";
        public String METHOD_POST = "POST";
        public String METHOD_PUT = "PUT";
        public String stylesNameRegEx = "<style>.*?<name>(.*?)</name>.*?</style>";
        public String workspaceNameRegEx = "<workspace>.*?<name>(.*?)</name>.*?</workspace>";
        public String datastoreNameRegEx = "<dataStore>.*?<name>(.*?)</name>.*?</dataStore>";
        public String coverageNameRegEx = "<coverage>.*?<name>(.*?)</name>.*?</coverage>";
        public String coverageStoreNameRegEx = "<coverageStore>.*?<name>(.*?)</name>.*?</coverageStore>";
        public String featuretypesNameRegEx = "<featureType>.*?<name>(.*?)</name>.*?</featureType>";
        public String layerNamesRegExPattern = "<layer>.*?<name>(.*?)</name>.*?</layer>";
        public String coverageNamesRegExPattern = "<coverage>.*?<name>(.*?)</name>.*?</coverage>";

        private String geoserverUrl = "http://localhost:8080/geoserver";
        private String username;
        private String password;

        public RestClient(String geoserverUrl)
        {
            this.geoserverUrl = geoserverUrl + "/rest";
            this.username = "admin";
            this.password = "geoserver";
        }

        public RestClient(String geoserverUrl, String username = "admin", String password = "geoserver")
        {
            this.geoserverUrl = geoserverUrl + "/rest";
            this.username = username;
            this.password = password;
        }

        public Boolean hasAuthorization()
        {
            return password != null && username != null;
        }

        private HttpWebResponse sendREST(String method, String urlAppend, byte[] postData, String contentType, String accept)
        {
            Boolean doOut = METHOD_DELETE != method;
            // boolean doIn = true; // !doOut

            HttpWebRequest req = GetGeoserverRestRequest(urlAppend);
            // uc.setDoInput(false);
            if (contentType != null && "" != contentType)
            {
                req.ContentType = contentType;
            }
            if (accept != null && "" != accept)
            {
                req.Accept = accept;
            }
            req.Method = method;

            if (hasAuthorization())
            {
                String userPasswordEncoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
                req.Headers["Authorization"] = "Basic " + userPasswordEncoded;
            }

            req.ContentLength = postData.Length;

            if (postData.Length > 0)
            {
                // Send the request:
                using (Stream post = req.GetRequestStream())
                {
                    post.Write(postData, 0, postData.Length);
                }
            }

            HttpWebResponse resp = null;
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException we)
            {
                HttpWebResponse errorResponse = we.Response as HttpWebResponse;
                if (errorResponse.StatusCode == HttpStatusCode.NotFound)
                {

                }
            }

            return resp;
        }

        /**
         * @param method e.g. 'POST', 'GET', 'PUT' or 'DELETE'
         * @param urlEncoded e.g. '/workspaces' or '/workspaces.xml'
         * @param contentType format of postData, e.g. null or 'text/xml'
         * @param accept format of response, e.g. null or 'text/xml'
         * @param postData e.g. xml data
         * @return null, or response of server
         */
        public int sendRESTint(String method, String urlEncoded, byte[] postData, String contentType, String accept)
        {
            HttpWebResponse resp = sendREST(method, urlEncoded, postData, contentType, accept);
            int status = 200;

            if (resp != null)
            {
                status = (int)resp.StatusCode;
            }

            return status;
        }

        public int sendRESTint(String method, String urlEncoded, String postData, String contentType, String accept)
        {
            byte[] bPostData = new byte[0];
            if (postData != null)
            {
                bPostData = UTF8Encoding.UTF8.GetBytes(postData);
            }
            return sendRESTint(method, urlEncoded, bPostData, contentType, accept);
        }

        public int sendRESTint(String method, String url, String xmlPostContent)
        {
            return sendRESTint(method, url, xmlPostContent, "application/xml", "application/xml");
        }

        /**
         * @param method e.g. 'POST', 'GET', 'PUT' or 'DELETE'
         * @param urlEncoded e.g. '/workspaces' or '/workspaces.xml'
         * @param contentType format of postData, e.g. null or 'text/xml'
         * @param accept format of response, e.g. null or 'text/xml'
         * @param postData e.g. xml data
         * @return null, or location field of the response header
         */
        public String sendRESTlocation(String method, String urlEncoded, byte[] postData, String contentType, String accept)
        {
            HttpWebResponse resp = sendREST(method, urlEncoded, postData, contentType, accept);
            return resp.Headers["Location"];
        }

        public String sendRESTlocation(String method, String urlEncoded, String postData, String contentType, String accept)
        {
            byte[] bPostData = new byte[0];
            if (postData != null)
            {
                bPostData = UTF8Encoding.UTF8.GetBytes(postData);
            }
            return sendRESTlocation(method, urlEncoded, bPostData, contentType, accept);
        }

        /**
         * Sends a REST request and return the answer as a String.
         * 
         * @param method e.g. 'POST', 'GET', 'PUT' or 'DELETE'
         * @param urlEncoded e.g. '/workspaces' or '/workspaces.xml'
         * @param contentType format of postData, e.g. null or 'text/xml'
         * @param accept format of response, e.g. null or 'text/xml'
         * @param is where to read the data from
         * @throws IOException
         * @return null, or response of server
         */
        public String sendRESTstring(String method, String urlEncoded, byte[] postData, String contentType, String accept)
        {
            HttpWebResponse resp = sendREST(method, urlEncoded, postData, contentType, accept);
            String result = "";
            Stream responseStream = resp.GetResponseStream();
            try
            {
                using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
            }
            finally
            {
                resp.Close();
            }

            return result;
        }

        public String sendRESTstring(String method, String urlEncoded, String postData, String contentType, String accept)
        {
            byte[] bPostData = new byte[0];
            if (postData != null)
            {
                bPostData = UTF8Encoding.UTF8.GetBytes(postData);
            }
            return sendRESTstring(method, urlEncoded, bPostData, contentType, accept);
        }

        public String sendRESTstring(String method, String url, String xmlPostContent)
        {
            return sendRESTstring(method, url, xmlPostContent, "application/xml", "application/xml");
        }

        /**
         * Works: curl -u admin:geoserver -v -XPUT -H 'Content-type: application/zip' --data-binary
         * 
         * @/home/stefan/Desktop/arabicData.zip http://localhost:8085/geoserver/rest/workspaces/ws/datastores/test1/file.shp
         */
        public String uploadShape(String workspace, String dsName, Uri zipUri)
        {
            String fileUri = zipUri.AbsolutePath;
            byte[] localShapeFile = readLocalShapeFile(fileUri);

            String returnString = sendRESTstring(METHOD_PUT, "/workspaces/" + workspace + "/datastores/" + dsName
                            + "/file.shp", localShapeFile, "application/zip", null);

            return returnString;
        }

        /**
         * @throws IOException
         */
        public Boolean createCoverage(String wsName, String csName, String cName)
        {
            String xml = "<coverage><name>" + cName + "</name><title>" + cName + "</title></coverage>";
            int status = sendRESTint(METHOD_POST, "/workspaces/" + wsName + "/coveragestores/" + csName + "/coverages", xml);

            return 201 == status;
        }

        public Boolean reload()
        {
            return 201 == sendRESTint(METHOD_POST, "/reload", null);
        }

        /**
         * This method does not upload a shapefile via zip. It rather creates a reference to a Shapefile that has already
         * exists in the GS data directory. <br/>
         * 
         * TODO: This is buggy and always puts the coveragestore in the default workspace. Therefore we set the default
         * workspace defore every command, and reset it afterwards. This will change the default workspace for a moment!
         * 
         * @param relpath
         *            A path to the file, relative to gsdata dir, e.g. "file:data/water.shp"
         */
        public Boolean createCoverageGeoTiff(String wsName, String csName, String csNamespace, String relpath, Configure autoConfig)
        {
            String oldDefault = getDefaultWs();

            try
            {
                setDefaultWs(wsName);

                if (relpath == null)
                    throw new Exception("parameter relpath may not be null");

                if (autoConfig == Configure.empty)
                    autoConfig = Configure.first;

                String urlParamter = "<url>" + relpath + "</url>";

                // String namespaceParamter = "<entry key=\"namespace\">" + dsName
                // + "</entry>";
                String typeParamter = "<type>GeoTIFF</type>";
                String xml = "<coverageStore><name>" + csName + "</name><enabled>true</enabled>" + typeParamter
                                + urlParamter + "</coverageStore>";

                int returnCode = sendRESTint(METHOD_POST, "/workspaces/" + wsName + "/coveragestores?configure="
                                + autoConfig.ToString(), xml);
                return 201 == returnCode;
            }
            catch (IOException e)
            {
                setDefaultWs(oldDefault);
                throw e;
            }
            finally
            {
                reload();
            }
        }

        public Boolean setDefaultWs(String wsName)
        {
            String xml = "<workspace><name>" + wsName + "</name></workspace>";
            return 200 == sendRESTint(METHOD_PUT, "/workspaces/default.xml", xml);
        }

        /**
         * Returns the name of the default workspace
         * 
         * @throws IOException
         */
        public String getDefaultWs()
        {
            String xml = sendRESTstring(METHOD_GET, "/workspaces/default", null);
            List<String> workspaces = parseXmlWithregEx(xml, workspaceNameRegEx);
            return workspaces[0];
        }

        public Boolean createDatastorePg(String workspace, String dsName, String dsNamespace, String host, String port,
                        String db, String user, String pwd, Boolean exposePKs)
        {
            String dbType = "postgis";
            return createDbDatastore(workspace, dsName, dsNamespace, host, port, db, user, pwd, dbType, exposePKs);
        }

        public Boolean createDatastoreShapefile(String workspace, String dsName, String dsNamespace, String relpath, String chartset)
        {
            return createDatastoreShapefile(workspace, dsName, dsNamespace, relpath, chartset, true, true, Configure.empty);
        }

        /**
         * − <dataStore> <name>xxx</name> <description>xxx</description> <type>Shapefile</type> <enabled>true</enabled> −
         * <workspace> <name>ws</name> <atom:link rel="alternate"
         * href="http://localhost:8085/geoserver/rest/workspaces/ws.xml" type="application/xml"/> </workspace> −
         * <connectionParameters> <entry key="memory mapped buffer">true</entry> <entry
         * key="create spatial index">true</entry> <entry key="charset">ISO-8859-1</entry> <entry
         * key="url">file:data/ad2/soils.shp</entry> <entry key="namespace">http://ws</entry> </connectionParameters> −
         * <featureTypes> <atom:link rel="alternate" href=
         * "http://localhost:8085/geoserver/rest/workspaces/ws/datastores/xxx/featuretypes.xml" type="application/xml"/>
         * </featureTypes> </dataStore>
         */
        /**
         * This method does not upload a shapefile via zip. It rather creates a reference to a Shapefile that has already
         * exists in the GS data directory.
         * 
         * @param charset
         *            defaults to UTF-8 if not set. Charset, that any text content is stored in.
         * 
         * @param relpath
         *            A path to the file, relative to gsdata dir, e.g. "file:data/water.shp"
         */
        public Boolean createDatastoreShapefile(String workspace, String dsName, String dsNamespace, String relpath,
                        String charset, Boolean memoryMappedBuffer, Boolean createSpatialIndex, Configure autoConfig)
        {
            if (autoConfig == Configure.first)
                autoConfig = Configure.empty;

            if (relpath == null)
                throw new Exception("parameter relpath may not be null");

            String createSpatialIndexParam = RestUtil.entryKey("create spatial index", createSpatialIndex);
            String memoryMappedBufferParamter = RestUtil.entryKey("memory mapped buffer", memoryMappedBuffer);
            String charsetParamter = "<entry key=\"charset\">" + (charset == null ? "UTF-8" : charset) + "</entry>";
            String urlParamter = "<entry key=\"url\">" + relpath + "</entry>";
            String namespaceParamter = "<entry key=\"namespace\">" + dsName + "</entry>";
            String typeParamter = "<type>Shapefile</type>";
            String xml = "<dataStore><name>" + dsName + "</name><enabled>true</enabled>" + typeParamter
                            + "<connectionParameters>" + createSpatialIndexParam + memoryMappedBufferParamter + charsetParamter
                            + urlParamter + namespaceParamter + typeParamter + "</connectionParameters></dataStore>";
            String configureParam = autoConfig == Configure.empty ? "" : "?configure=" + autoConfig.ToString();

            int returnCode = sendRESTint(METHOD_POST, "/workspaces/" + workspace + "/datastores.xml" + configureParam, xml);
            return 201 == returnCode;
        }

        public Boolean createDbDatastore(String workspace, String dsName, String dsNamespace, String host, String port,
                        String db, String user, String pwd, String dbType, Boolean exposePKs)
        {
            String exposePKsParamter = "<entry key=\"Expose primary keys\">" + exposePKs + "</entry>";
            String xml = "<dataStore><name>" + dsName + "</name><enabled>true</enabled><connectionParameters><host>" + host
                            + "</host><port>" + port + "</port><database>" + db + "</database><user>" + user + "</user><passwd>"
                            + pwd + "</passwd><dbtype>" + dbType + "</dbtype><namespace>" + dsNamespace + "</namespace>"
                            + exposePKsParamter + "</connectionParameters></dataStore>";

            int returnCode = sendRESTint(METHOD_POST, "/workspaces/" + workspace + "/datastores", xml);
            return 201 == returnCode;
        }

        /**
         * Create a <em>Featuretype</em> based on an existing datastore.
         * 
         * @param wsName the GeoServer workspace name
         * @param dsName the GeoServer datastore name
         * @param ftName the featureTypeName you want to create, e.g. the name of a PostGIS table or the name of a Shapefile (without .shp)
         * @param srs <code>null</code> or <code>EPSG:????</code> syntax.
         * @param nativeWKT <code>null</code> or WKT declaration of the CRS.
         * 
         * @return <code>true</code> if the creation was successful.
         */
        public Boolean createFeatureType(String wsName, String dsName, String ftName, String srs, String nativeWKT)
        {
            String nameTitleParam = "<name>" + ftName + "</name><title>" + ftName + "</title>";
            String enabledTag = "<enabled>" + true + "</enabled>";
            String srsTag = srs != null ? "<srs>" + srs + "</srs>" : "";
            String nativeCrsTag = nativeWKT != null ? "<nativeCRS>" + nativeWKT + "</nativeCRS>" : "";
            String prjPolTag = "<projectionPolicy>FORCE_DECLARED</projectionPolicy>";
            String xml = "<featureType>" + nameTitleParam + srsTag + prjPolTag + enabledTag + nativeCrsTag
                            + "</featureType>";

            int status = sendRESTint(METHOD_POST, "/workspaces/" + wsName + "/datastores/" + dsName + "/featuretypes",
                            xml);
            return 201 == status;
        }

        /**
         * Uploads an SLD to the Geoserver
         * 
         * @param stylename
         * @param sldString SLD-XML as String
         * @return <code>true</code> successfully uploaded
         */
        public Boolean createSld(String stylename, String sldString)
        {
            return null != createSld_location(stylename, sldString);
        }

        /**
         * Add an existing style to a layer.
         * 
         * @param styleName name of the stlye to associate with the layer.
         * @param layername name of the layer to associate with the style.
         * @return <code>true</code> for operation success.
         * 
         * @see #createSld
         */
        public Boolean addStyleToLayer(String styleName, String layername)
        {
            return addStyleToLayer(styleName, layername, false);
        }

        /**
         * Add an existing style to a layer.
         * 
         * @param styleName name of the stlye to associate with the layer.
         * @param layername name of the layer to associate with the style.
         * @param asDefault Set <code>true</code> if this shall be setup as a default style
         * @return <code>true</code> for operation success.
         * 
         * @see #createSld
         */
        public Boolean addStyleToLayer(String styleName, String layername, Boolean asDefault)
        {
            String xml = "<style><name>" + styleName + "</name></style>";
            int result = sendRESTint(METHOD_POST, "/layers/" + layername + "/styles.xml", xml);
            if (result != 201)
                return false;

            if (asDefault)
            {
                xml = "<layer><defaultStyle><name>" + styleName + "</name></defaultStyle><enabled>true</enabled></layer>";
                return 200 == sendRESTint(METHOD_PUT, "/layers/" + layername, xml);
            }
            return true;
        }

        public List<String> getStylesForLayer(String layername)
        {
            String xml = sendRESTstring(METHOD_GET, "/layers/" + layername + "/styles", null);
            return parseXmlWithregEx(xml, stylesNameRegEx);
        }

        /**
         * @param stylename
         * @param sldString
         * @return REST location URL string to the new style
         * @throws IOException
         */
        public String createSld_location(String stylename, String sldString)
        {
            String location = sendRESTlocation(METHOD_POST, "/styles/" + "?name=" + stylename, sldString, "application/vnd.ogc.sld+xml", "application/vnd.ogc.sld+xml");
            return location;
        }

        public Boolean createWorkspace(String workspaceName)
        {
            return 201 == sendRESTint(METHOD_POST, "/workspaces", "<workspace><name>" + workspaceName + "</name></workspace>");
        }

        /**
         * Deletes a datastore
         * 
         * @param wsName name of the workspace
         * @param dsName name of the datastore
         * @param recusively delete all contained featureytpes also
         */
        public Boolean deleteDatastore(String wsName, String dsName, Boolean recusively)
        {
            if (recusively == true)
            {
                List<String> layerNames = getLayersUsingDataStore(wsName, dsName);

                foreach (String lName in layerNames)
                {
                    if (!deleteLayer(lName))
                        throw new Exception("Could not delete layer " + wsName + ":" + dsName + ":" + lName);
                }
                if (getDatastores(wsName).Contains(dsName))
                {
                    List<String> ftNames = getFeatureTypes(wsName, dsName);
                    foreach (String ftName in ftNames)
                    {
                        // it happens that this returns false, e.g maybe for
                        // notpublished featuretypes!?
                        deleteFeatureType(wsName, dsName, ftName);
                    }
                }
            }
            return 200 == sendRESTint(METHOD_DELETE, "/workspaces/" + wsName + "/datastores/" + dsName, null);
        }

        /**
         * Deletes a coveragestore
         * 
         * @param wsName name of the workspace
         * @param csName name of the coveragestore
         * @param recusively delete all contained coverages also
         */
        public Boolean deleteCoveragestore(String wsName, String csName, Boolean recusively)
        {
            if (recusively == true)
            {
                deleteLayersUsingCoveragestore(wsName, csName);

                List<String> covNames = getCoverages(wsName, csName);
                //
                foreach (String ftName in covNames)
                {
                    // it happens that this returns false, e.g maybe for
                    // notpublished featuretypes!?
                    deleteCoverage(wsName, csName, ftName);
                }
            }
            return 200 == sendRESTint(METHOD_DELETE, "/workspaces/" + wsName + "/coveragestores/" + csName, null);
        }

        private void deleteLayersUsingCoveragestore(String wsName, String csName)
        {
            List<String> layerNames = getLayersUsingCoverageStore(wsName, csName);

            foreach (String lName in layerNames)
            {
                if (!deleteLayer(lName))
                    throw new Exception("Could not delete layer " + lName);
            }
        }

        public Boolean deleteCoverage(String wsName, String csName, String covName)
        {
            deleteLayersUsingCoveragestore(wsName, csName);
            int result = sendRESTint(METHOD_DELETE, "/workspaces/" + wsName + "/coveragestores/" + csName + "/coverages/" + covName, null);

            return result == 200;
        }

        /**
         * To avoid
         * "org.geoserver.rest.RestletException: java.lang.IllegalArgumentException: Unable to delete resource referenced by layer"
         * use deleteLayer first.
         */
        public Boolean deleteFeatureType(String wsName, String dsName, String ftName)
        {
            try
            {
                return sendRESTint(METHOD_DELETE, "/workspaces/" + wsName + "/datastores/" + dsName + "/featuretypes/" + ftName, null) == 200;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Boolean deleteLayer(String lName)
        {
            try
            {
                int result = sendRESTint(METHOD_DELETE, "/layers/" + lName, null);
                return result == 200;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Boolean deleteSld(String styleName, Boolean purge)
        {
            int result = sendRESTint(METHOD_DELETE, "/styles/" + styleName + ".sld?purge=" + purge.ToString(), null);
            // + "&name=" + styleName
            return result == 200;
        }

        /**
         * Deletes a workspace recursively.
         * 
         * @param wsName name of the workspace to delete recursively.
         */
        public Boolean deleteWorkspace(String wsName)
        {
            return deleteWorkspace(wsName, true);
        }

        /**
         * Deletes a workspace recursively. If the workspace could not be deleted (e.g. didn't exist, or not recursively
         * deleting and not empty) returns <code>false</code>
         * 
         * @param wsName name of the workspace to delete, including all content.
         */
        public Boolean deleteWorkspace(String wsName, Boolean recursive)
        {
            try
            {
                if (recursive)
                {
                    reload();

                    // Selete all datastores
                    // recusively
                    List<String> datastores = getDatastores(wsName);
                    foreach (String dsName in datastores)
                    {
                        if (!deleteDatastore(wsName, dsName, true))
                            throw new IOException("Could not delete dataStore " + dsName + " in workspace " + wsName);
                    }

                    // Selete all datastores
                    // recusively
                    List<String> coveragestores = getCoveragestores(wsName);
                    foreach (String csName in coveragestores)
                    {
                        if (!deleteCoveragestore(wsName, csName, true))
                            throw new IOException("Could not delete coverageStore " + csName + " in workspace " + wsName);
                    }

                }

                return 200 == sendRESTint(METHOD_DELETE, "/workspaces/" + wsName, "", "application/xml", "application/xml");
            }
            catch (Exception)
            {
                // Workspace didn't exist
                return false;
            }
            finally
            {
                try
                {
                    reload();
                }
                catch (IOException)
                {
                }
            }
        }

        /**
         * A list of coveragestores
         */
        public List<String> getCoveragestores(String wsName)
        {
            try
            {
                String coveragesXml = sendRESTstring(METHOD_GET, "/workspaces/" + wsName + "/coveragestores.xml", null);
                List<String> coveragestores = parseXmlWithregEx(coveragesXml, coverageStoreNameRegEx);
                return coveragestores;
            }
            catch (Exception)
            {
                return new List<String>();
            }
        }

        /**
         * A list of datastorenames
         */
        public List<String> getDatastores(String wsName)
        {
            try
            {
                String datastoresXml = sendRESTstring(METHOD_GET, "/workspaces/" + wsName + "/datastores.xml", null);
                List<String> datastores = parseXmlWithregEx(datastoresXml, datastoreNameRegEx);
                return datastores;
            }
            catch (Exception)
            {
                return new List<String>();
            }
        }

        /**
         * A list of all workspaces
         */
        public List<String> getWorkspaces()
        {
            try
            {
                String xml = sendRESTstring(METHOD_GET, "/workspaces", null);
                List<String> workspaces = parseXmlWithregEx(xml, workspaceNameRegEx);
                return workspaces;
            }
            catch (IOException)
            {
                return new List<String>();
            }
        }

        /**
         * Tell this instance of {@link GsRest} to not use authorization
         */
        public void disableAuthorization()
        {
            this.password = null;
            this.username = null;
        }

        /**
         * Tell this {@link GsRest} instance to use authorization
         * 
         * @param username cleartext username
         * @param password cleartext password
         */
        public void enableAuthorization(String username, String password)
        {
            this.password = password;
            this.username = username;
        }

        public String getDatastore(String wsName, String dsName)
        {
            return sendRESTstring(METHOD_GET, "/workspaces/" + wsName + "/datastores/" + dsName, null);
        }

        public String getFeatureType(String wsName, String dsName, String ftName)
        {
            return sendRESTstring(METHOD_GET, "/workspaces/" + wsName + "/datastores/" + dsName + "/featuretypes/" + ftName, null);
        }

        /**
         * Returns a list of all featuretypes inside a a datastore
         * 
         * @param wsName
         * @param dsName
         */
        public List<String> getFeatureTypes(String wsName, String dsName)
        {
            try
            {
                String xml = sendRESTstring(METHOD_GET, "/workspaces/" + wsName + "/datastores/" + dsName + "/featuretypes", null);
                return parseXmlWithregEx(xml, featuretypesNameRegEx);
            }
            catch (Exception)
            {
                return new List<String>();
            }
        }

        /**
         * Returns a {@link List} of all layer names
         * 
         * @param wsName
         */
        public List<String> getLayerNames()
        {
            String xml = sendRESTstring(METHOD_GET, "/layers", null);
            return parseXmlWithregEx(xml, layerNamesRegExPattern);
        }

        /**
         * Returns a list of all coverageNames inside a a coveragestore
         */
        public List<String> getCoverages(String wsName, String csName)
        {
            try
            {
                String xml = sendRESTstring(METHOD_GET, "/workspaces/" + wsName + "/coveragestores/" + csName + "/coverages", null);

                return parseXmlWithregEx(xml, coverageNameRegEx);
            }
            catch (Exception)
            {
                return new List<String>();
            }
        }

        /**
         * Returns a list of all layers using a specific dataStore
         */

        public List<String> getLayersUsingCoverageStore(String wsName, String csName)
        {
            try
            {
                String pattern = "<layer>.*?<name>(.*?)</name>.*?/rest/workspaces/" + wsName + "/coveragestores/" + csName + "/coverages/.*?</layer>";

                List<String> coveragesUsingStore = new List<String>();
                foreach (String cName in getLayerNames())
                {
                    String xml = sendRESTstring(METHOD_GET, "/layers/" + cName, null);
                    // System.out.println(xml);
                    Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

                    Match m = r.Match(xml);
                    if (m.Success)
                        coveragesUsingStore.Add(cName);
                }

                return coveragesUsingStore;
            }
            catch (Exception)
            {
                return new List<String>();
            }
        }

        /**
         * Returns a list of all layers using a specific dataStore
         */
        public List<String> getLayersUsingDataStore(String wsName, String dsName)
        {
            try
            {
                String layersUsingStoreRegEx = "<layer>.*?<name>(.*?)</name>.*?/rest/workspaces/"
                                + wsName + "/datastores/" + dsName + "/featuretypes/.*?</layer>";

                List<String> layersUsingDs = new List<String>();
                foreach (String lName in getLayerNames())
                {
                    String xml = sendRESTstring(METHOD_GET, "/layers/" + lName, null);
                    // System.out.println(xml);
                    Regex r = new Regex(layersUsingStoreRegEx, RegexOptions.Singleline);

                    Match m = r.Match(xml);
                    if (m.Success)
                        layersUsingDs.Add(lName);
                }

                return layersUsingDs;
            }
            catch (Exception)
            {
                return new List<String>();
            }
        }

        /**
         * @return A list of all stylenames stored in geoserver. Includes "default" stylenames like <code>point</code>,
         *         <code>line</code>,etc.
         */
        public List<String> getStyles()
        {
            String xml = sendRESTstring(METHOD_GET, "/styles", null);
            return parseXmlWithregEx(xml, stylesNameRegEx);
        }

        private HttpWebRequest GetGeoserverRestRequest(String urlAppend)
        {
            return WebRequest.Create(new Uri(geoserverUrl + urlAppend)) as HttpWebRequest;
        }

        private byte[] readLocalShapeFile(string filePath)
        {
            byte[] buffer;
            FileStream fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                int length = (int)fStream.Length;
                buffer = new byte[length];
                int count;
                int sum = 0;

                while ((count = fStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;
            }
            finally
            {
                fStream.Close();
            }

            return buffer;
        }

        private List<String> parseXmlWithregEx(String xml, String pattern)
        {
            List<String> list = new List<String>();
            Regex nameMatcher = new Regex(pattern, RegexOptions.Singleline);
            Match m = nameMatcher.Match(xml);

            while (m.Success)
            {
                String name = m.Groups[1].Value;
                list.Add(name.Trim());
                m = m.NextMatch();
            }

            return list;
        }

        public Boolean purgeSld(String styleName)
        {
            return deleteSld(styleName, true);
        }
    }
}
