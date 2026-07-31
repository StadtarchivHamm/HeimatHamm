using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.Threading;
using System.IO;
#if !UNITY_WEBGL || UNITY_EDITOR
using Mono.Data.Sqlite;
#endif
using UnityEngine;
using System.Globalization;
#if !UniRxLibrary
using UniRx.ObservableWebRequest;
#endif

namespace Wezit
{
	public class SqlManager
	{
        #region Fields
		private bool online;
		private string identification;
		private string apiBaseUrl;
		
		private Dictionary<string, string> headers = new Dictionary<string, string>();
		
		private string db_connection_string
		{
			get => "URI=file:" + FilesDownloader.SqliteFullPath;
		}
        #endregion


        #region Methods
        #region Constructor
		public SqlManager()
		{
			// Add basic authentification
			headers.Add("Authorization", "Basic " + btoa("wsqlwezitio:r6U8Avd8"));

			// Create identification
			online = Config.WezitConfig.online;
			string version = Config.WezitConfig.version;
			string entityId = ManifestLoader.EntityId;
			string inventoryId = ManifestLoader.InventoryId.Replace("wzobj:inventory_", "");

			identification = "?type=" + version + "&weid=" + entityId + "&wiid=" + inventoryId;

			// Get api base url
			apiBaseUrl = "https://api.wezit.io/wsql/api/1.3/";
		}
        #endregion

        #region Public
		public IObservable<string> GetInventoryList()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT n.pid, dc_language AS language, " +
					" (CASE WHEN n.rte_title IS NULL OR n.rte_title = '' " +
					"THEN n.dc_title " +
					"ELSE n.rte_title END) AS title, " +
					" (CASE WHEN rte_subject IS NULL OR rte_subject = '' " +
					"THEN dc_subject " +
					"ELSE rte_subject END) AS subject, " +
					" (CASE WHEN rte_desc IS NULL OR rte_desc = '' " +
					"THEN dc_description " +
					"ELSE rte_desc END) AS description, " +
					"n.aspects, " +
					"n.tags " +
					" FROM OBJECT o " +
					" INNER JOIN NODE n ON n.pid = o.pid " +
					" LEFT JOIN RELATION r ON r.pid_src = n.pid " +
					"LEFT JOIN OBJECT_SOURCE os ON os.pid = r.pid_dest " +
					" WHERE o.type = 'WzNodeInventory' " +
					" GROUP BY n.pid, dc_language";
				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "inventory/list";
				return apiCall(urlApi);
			}
#else
			string urlApi = "inventory/list";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetInventoryById(string id)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT " +
					"NODE.pid, " +
					"(CASE WHEN rte_title IS NULL OR rte_title = '' " +
					"THEN dc_title " +
					"ELSE rte_title END) AS title, " +
					" (CASE WHEN rte_subject IS NULL OR rte_subject = '' " +
					"THEN dc_subject " +
					"ELSE rte_subject END) AS subject, " +
					"(CASE WHEN rte_desc IS NULL OR rte_desc = '' " +
					"THEN dc_description " +
					"ELSE rte_desc END) AS description, " +
					"aspects, " +
					"tags, " +
					"identifier, " +
					"dc_contributor as contributor, " +
					"dc_author as author, " +
					"dc_creator as creator, " +
					"dc_rights as rights, " +
					"dc_format AS format, " +
					"dc_publisher AS publisher, " +
					"dc_type AS type, " +
					"dc_language AS language, " +
					"dc_date AS date, " +
					"dc_source AS source, " +
					"dc_location AS location, " +
					"dc_spatial AS spatial " +
					"dc_location AS location " +
					"FROM NODE " +
					"JOIN IDENTIFIER " +
					"WHERE NODE.pid = \"" + id + "\"AND IDENTIFIER.pid = \"" + id + "\"";

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "inventory/" + id;
				return apiCall(urlApi);
			}
#else
			string urlApi = "inventory/" + id;
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetTourList()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT DISTINCT " +
					"n.pid, " +
					"language, " +
					" (CASE WHEN n.rte_title IS NULL OR n.rte_title = '' " +
					"THEN n.dc_title " +
					"ELSE n.rte_title END) AS title, " +
					"(CASE WHEN rte_subject IS NULL OR rte_subject = '' " +
					"THEN dc_subject " +
					"ELSE rte_subject END) AS subject, " +
					" (CASE WHEN rte_desc IS NULL OR rte_desc = '' " +
					"THEN dc_description " +
					"ELSE rte_desc END) AS description, " +
					"n.aspects, " +
					"n.tags, " +
					"n.dc_format AS format, " +
					"n.dc_type AS type " +
					"FROM VIEW_TOUR_ID vt " +
					"INNER JOIN NODE n ON n.pid = vt.node_pid " +
					"LEFT JOIN RELATION r ON r.pid_src = n.pid";
				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "tour/list";
				return apiCall(urlApi);
			}
#else
			string urlApi = "tour/list";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetTourById(string id)
		{
			return GetInventoryById(id);
		}

		public IObservable<string> GetPoiList()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT DISTINCT " +
					"NODE.pid, " +
					"(CASE WHEN rte_title IS NULL OR rte_title = '' " +
					"THEN dc_title " +
					"ELSE rte_title END) AS title, " +
					" (CASE WHEN rte_subject IS NULL OR rte_subject = '' " +
					"THEN dc_subject " +
					"ELSE rte_subject END) AS subject, " +
					" (CASE WHEN rte_desc IS NULL OR rte_desc = '' " +
					"THEN dc_description " +
					"ELSE rte_desc END) AS description, " +
					"aspects, " +
					"tags, " +
					"   '[' || group_concat('{ \"pid\" : \"' || coalesce( relation.pid, \"\" ) || '\", \"relationName\": \"' || coalesce( relation.relationName, \"\" ) || '\", \"order\": ' || coalesce(relation.ord, 0) || ' }') || ']' as relationList, " +
					"dc_identifier AS identifier, " +
					"dc_format AS format, " +
					"dc_publisher AS publisher, " +
					"dc_contributor as contributor, " +
					"dc_author as author, " +
					"dc_creator as creator, " +
					"dc_rights as rights, " +
					"dc_type AS type, " +
					"dc_language AS language, " +
					"dc_date AS date, " +
					"dc_source AS source, " +
					"dc_spatial AS spatial, " +
					"dc_location AS location, " +
					"dc_extent AS extent " +
					"FROM " +
					"( " +
					"SELECT pid_src, pid_dest as pid, relation_name as relationName, ord " +
					"FROM RELATION " +
					"ORDER BY ord  " +
					") as relation " +
					"NATURAL JOIN IDENTIFIER " +
					"JOIN NODE ON relation.pid_src = NODE.pid " +
					"WHERE (NODE.pid LIKE \"%wzobj:scenode%\" OR NODE.pid LIKE \"%wzobj:poi%\") " +
					"GROUP BY NODE.pid";
				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "poi/list";
				return apiCall(urlApi);
			}
#else
			string urlApi = "poi/list";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetPoiListByTourId(string id)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT " +
					"n.pid, " +
					"dc.type, " +
					"dc.language, " +
					"dc.identifier, " +
					" (CASE WHEN n.rte_title IS NULL OR n.rte_title = '' " +
					"THEN n.dc_title " +
					"ELSE n.rte_title END) AS title, " +
					" (CASE WHEN rte_subject IS NULL OR rte_subject = '' " +
					"THEN dc_subject " +
					"ELSE rte_subject END) AS subject, " +
					" (CASE WHEN rte_desc IS NULL OR rte_desc = '' " +
					"THEN dc_description " +
					"ELSE rte_desc END) AS description, " +
					"n.aspects, " +
					"n.tags " +
					"FROM RELATION r " +
					"JOIN NODE n ON n.pid = r.pid_dest " +
					"JOIN DUBLIN_CORE dc on dc.pid = r.pid_dest " +
					"WHERE r.pid_src = \"" + id + "\" AND relation_name = \"hasNode\"";

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "tour/" + id + "/poi/list/noimages";
				return apiCall(urlApi);
			}
#else
			string urlApi = "tour/" + id + "/poi/list/noimages";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetPoiById(string id)
		{
			return GetInventoryById(id);
		}

		public IObservable<string> GetPoiDublinCoreById(string id)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT dc.pid, dc.contributor, dc.creator, dc.date, dc.description, dc.format, dc.publisher, dc.rights, dc.source, dc.subject, dc.title, dc.type, dc.identifier, dc.language, dc.author " +
					"FROM DUBLIN_CORE as dc " +
					"WHERE dc.pid = \"" + id + "\"";

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "poi/" + id + "/dublin-core";
				return apiCall(urlApi);
			}
#else
			string urlApi = "poi/" + id + "/dublin-core";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetRelationListByPoiId(string language, string id)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT node_pid as pid, name as relationName, aspects, is_subject, language " +
					"FROM VIEW_RELATIONS " +
					"WHERE (language like \"" + language + "\" OR language IS NULL) AND parent_pid = \"" + id + "\" AND is_subject = 0 " +
					"ORDER BY relationName;";

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = language + "/poi/" + id + "/relation/list";
				return apiCall(urlApi);
			}
#else
			string urlApi = language + "/poi/" + id + "/relation/list";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetArtworkByPoiId(string id)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT pid, title, type, date_creation, domain, subdomain, dimensions, inventory_reference, " +
					"acquisition, bibliography, graphic_description, historic_description, society_description, usage_description, author " +
					"FROM ARTWORK " +
					" WHERE pid = \"" + id + "\";";

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "poi/" + id + "/artwork";
				return apiCall(urlApi);
			}
#else
			string urlApi = "poi/" + id + "/artwork";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetAssetListByNodeId(string nodeType, string id)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					 "SELECT ra.pid, ra.title, ra.subject, ra.description, ra.tags, ra.relation, ra.ord, " +
					 "'[' || GROUP_CONCAT('{ " +
					 "\"label\" : \"' || label || '\", " +
					 "\"url\" : \"' || coalesce( url, \"\" ) || '\", " +
					 "\"path\": \"' || coalesce( os.path, \"\" ) || '\", " +
					 "\"pid\": \"' || os.pid || '\", " +
					 "\"metadata\": \"' || coalesce( os.metadata, \"\" ) || '\", " +
					 "\"mimeType\": \"' || os.mimeType || '\"' || ' }') || ']' " +
					 "as assets, ra.usage, ra.language, dc.type as type, dc.rights as rights, dc.source as source " +
					 "FROM VIEW_RELATED_ASSET ra " +
					 "JOIN DUBLIN_CORE dc ON ra.pid = dc.pid " +
					 "INNER JOIN OBJECT_SOURCE os ON os.pid = ra.pid " +
					 "WHERE related_pid = \"" + id + "\" GROUP BY ra.usage, relation;";

				return dbCall(dbcmd.ExecuteReader(), db_connection);

			}
			else
			{
				string urlApi = nodeType + "/" + id + "/asset/list";
				return apiCall(urlApi);
			}
#else
			string urlApi = nodeType + "/" + id + "/asset/list";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetLocationList()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
                    "SELECT relation.pid_src AS tour_pid, node.pid, node.dc_language as language, po.x as x, po.y as y, " +
                    "geo.latitude as lat, geo.longitude as lng, po.relX as relX, po.relY as relY, geo.radius as radius, coalesce(vta.title, geo.map) as map_name, geo.map as map_pid, vta.subject as subject, vta.description as description, " +
                    "'[' || GROUP_CONCAT('{\"pid\" : \"' || vta.pid || '\", \"metadata\" : \"' || coalesce(vta.metadata, \"\") || '\", \"label\" : \"' || vta.transformation || '\", \"url\" : \"' || coalesce(vta.uri, \"\") || '\", \"path\": \"' || vta.path || '\"}') || ']' as maps " +
                    "FROM RELATION relation " +
                    "INNER JOIN NODE AS node ON relation.pid_dest = node.pid AND relation.relation_name = \"hasNode\" " +
                    "LEFT JOIN POSITION po ON po.pid = node.pid " +
                    "LEFT JOIN(SELECT* FROM VIEW_GEODATA UNION SELECT* FROM VIEW_TRANSFORMED_GEODATA) as geo ON node.pid = geo.pid " +
                    "LEFT JOIN VIEW_TRANSFORMED_ASSET vta ON(vta.pid = po.map_name OR vta.pid = geo.map) " +
                    "where po.x is not null and po.y is not null OR geo.latitude and geo.longitude is not null " +
                    "GROUP BY tour_pid, node.pid, po.map_name;";
                return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "location/list";
				return apiCall(urlApi);
			}
#else
			string urlApi = "location/list";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetGeolocationList()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT " +
					" distinct(geo.pid) as pid, " +
					"geo.lat as latitude, " +
					"geo.lng as longitude, " +
					"geo.radius as radius " +
					"FROM GEOPOSITION geo " +
					"GROUP BY geo.pid";

				return dbCall(dbcmd.ExecuteReader(), db_connection, true);
			}
			else
			{
				string urlApi = "geolocation/list";
				return apiCall(urlApi);
			}
#else
			string urlApi = "geolocation/list";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetGlossaryList(string language)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT * FROM GLOSSARY WHERE language like \"" + language + "\" OR language IS NULL ORDER BY UPPER(word)";

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = language + "/glossary/list";
				return apiCall(urlApi);
			}
#else
			string urlApi = language + "/glossary/list";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetGlossaryById(string id)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT * FROM GLOSSARY WHERE pid = " + id;

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "glossary/" + id;
				return apiCall(urlApi);
			}
#else
			string urlApi = "glossary/" + id;
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetRelationSubjectByPoiId(string language, string id)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT node_pid as pid, name as relationName, aspects, is_subject, language " +
					"FROM VIEW_RELATIONS " +
					"WHERE (language like \"" + language + "\" OR language IS NULL) AND parent_pid = \"" + id + "\" AND is_subject = \"" + id +
					"\"ORDER BY relationName;";

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = language + "poi/" + id + "relation/list/issubject";
				return apiCall(urlApi);
			}
#else
			string urlApi = language + "/poi/" + id + "/relation/list/issubject";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetIdentifierList(string id)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT IDENTIFIER.pid " +
					"FROM IDENTIFIER, DUBLIN_CORE " +
					" WHERE " +
					"IDENTIFIER.pid = DUBLIN_CORE.pid " +
					"AND IDENTIFIER.identifier = \"" + id +
					"\" AND (DUBLIN_CORE.language is NULL OR DUBLIN_CORE.language like \"" + id + "\")";

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "identifier/" + id;
				return apiCall(urlApi);
			}
#else
			string urlApi = "identifier/" + id;
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetAssetById(string id)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT os.pid, os.label, os.metadata, os.mimetype, os.path, a.dc_language AS language, " +
					"(CASE WHEN rte_subject IS NULL OR rte_subject = '' THEN dc_subject ELSE rte_subject END) AS subject, " +
					"(CASE WHEN rte_desc IS NULL OR rte_desc = '' THEN dc_description ELSE rte_desc END) AS description " +
					"FROM OBJECT_SOURCE os " +
					"LEFT JOIN ASSET a on a.pid = os.pid " +
					"WHERE os.pid = \"" + id + "\"";

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "asset/" + id;
				return apiCall(urlApi);
			}
#else
			string urlApi = "asset/" + id;
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetVersionByNode(string id)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					" SELECT distinct(r.parent_pid) AS pid, dc.language " +
					" FROM VIEW_RELATIONS r " +
					" LEFT JOIN DUBLIN_CORE dc on dc.pid = r.parent_pid " +
					" WHERE r.name = 'hasVersion' AND r.node_pid = \"" + id + "\"";

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "node/" + id + "/version";
				return apiCall(urlApi);
			}
#else
			string urlApi = "node/" + id + "/version";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetConfigurationFileByNode(string id)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT path FROM RELATION LEFT JOIN OBJECT_SOURCE on pid = pid_dest WHERE pid_src = \"" + id + "\" AND relation_name = \"relationForSetConfigurationFile\"";

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "node/" + id + "/configuration";
				return apiCall(urlApi);
			}
#else
			string urlApi = "node/" + id + "/configuration";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetAspectsByPoi(string aspectsName, string poiId)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText =
					"SELECT * " +
					"FROM " + aspectsName + " " +
					"WHERE pid = \"" + poiId + "\"";

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "Poi/" + poiId + "/" + aspectsName;
				return apiCall(urlApi);
			}
#else
			string urlApi = "Poi/" + poiId + "/" + aspectsName;
			return apiCall(urlApi);
#endif
		}

#if !UNITY_WEBGL || UNITY_EDITOR
		public IObservable<string> GetTransformedAssets(string transformation)
		{
			IDbConnection db_connection = new SqliteConnection(db_connection_string);
			db_connection.Open();
			IDbCommand dbcmd = db_connection.CreateCommand();
			if (transformation == "all")
			{
				dbcmd.CommandText = "SELECT pid, language, md5, path, uri, size, relation, related_pid, title, transformation, mimeType FROM VIEW_RELATED_TRANSFORMED_ASSET";
			}
			else dbcmd.CommandText = "SELECT pid, language, md5, path, uri, size, relation, related_pid, title, transformation, mimeType FROM VIEW_RELATED_TRANSFORMED_ASSET WHERE transformation = '" + transformation + "'";

			return dbCall(dbcmd.ExecuteReader(), db_connection);
		}

		public IObservable<string> GetActivitiesForTour(Tour tour)
		{
			IDbConnection db_connection = new SqliteConnection(db_connection_string);
			db_connection.Open();
			IDbCommand dbcmd = db_connection.CreateCommand();
			dbcmd.CommandText = "SELECT transformed_asset.pid as asset_pid FROM VIEW_RELATED_TRANSFORMED_ASSET as transformed_asset JOIN VIEW_TOUR_POI as tour_poi " +
				"ON tour_poi.pid = transformed_asset.related_pid WHERE tour_poi.tour_pid = "
				+ tour.pid + " AND transformed_asset.type = \"WzAssetActivity\"";

			return dbCall(dbcmd.ExecuteReader(), db_connection);
		}
#endif

		public IObservable<string> Get3DPositions()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText = "SELECT relation.pid_src as tour_pid, node.pid, node.dc_language as language, po.startTime as startTime, " +
					"po.endTime as endTime, po.yaw as yaw, po.pitch as pitch, po.roll as roll, po.fov as fov, " +
					"coalesce(vta.title, po.map_name) as map_name, vta.subject as subject, vta.description as description, " +
					"'[' || GROUP_CONCAT('{ \"pid\" : \"' || coalesce(vta.pid, \"\") || '\", \"metadata\" : \"' || coalesce(vta.metadata, \"\") || '\", \"label\" : \"' || coalesce(vta.transformation, \"\") || '\", \"url\" : \"' || coalesce(vta.uri, \"\") || '\", \"path\": \"' || coalesce(vta.path, \"\") || '\"}') || ']' as maps " +
					"FROM RELATION relation INNER JOIN NODE AS node ON relation.pid_dest = node.pid AND relation.relation_name = \"hasNode\" " +
					"LEFT JOIN THREED_POSITION po ON po.pid = node.pid LEFT JOIN VIEW_TRANSFORMED_ASSET vta ON po.map_name = vta.pid where po.yaw is not null and po.pitch is not null " +
					"GROUP BY tour_pid, node.pid, po.map_name";
				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "locations/3d";
				return apiCall(urlApi);
			}
#else
			string urlApi = "locations/3d";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetCategoryList()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText = "select *, CATEGORY.pid as pid  from CATEGORY inner join DUBLIN_CORE on CATEGORY.categoryId = DUBLIN_CORE.pid";
				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "categories";
				return apiCall(urlApi);
			}
#else
			string urlApi = "categories";
			return apiCall(urlApi);
#endif
		}
		public IObservable<string> GetCategoryById(string nodeId)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText = "select *, CATEGORY.pid as pid  from CATEGORY inner join DUBLIN_CORE on CATEGORY.categoryId = DUBLIN_CORE.pid where CATEGORY.pid = " + nodeId;

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "categories/" + nodeId;
				return apiCall(urlApi);
			}
#else
			string urlApi = "categories/" + nodeId;
			return apiCall(urlApi);
#endif
		}
		public IObservable<string> GetAttributesByNodeId(string nodeId)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText = "select key, value from PROP_GENERICS inner join DUBLIN_CORE on PROP_GENERICS.pid = DUBLIN_CORE.pid	where PROP_GENERICS.pid = " + nodeId;

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "node/" + nodeId + "attributes";
				return apiCall(urlApi);
			}
#else
			string urlApi = "node/" + nodeId + "attributes/";
			return apiCall(urlApi);
#endif
		}

		public IObservable<string> GetAttributes()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				IDbConnection db_connection = new SqliteConnection(db_connection_string);
				db_connection.Open();
				IDbCommand dbcmd = db_connection.CreateCommand();
				dbcmd.CommandText = "select key, value, PROP_GENERICS.pid as pid from PROP_GENERICS inner join DUBLIN_CORE on PROP_GENERICS.pid = DUBLIN_CORE.pid";

				return dbCall(dbcmd.ExecuteReader(), db_connection);
			}
			else
			{
				string urlApi = "attributes/";
				return apiCall(urlApi);
			}
#else
			string urlApi = "attributes/";
			return apiCall(urlApi);
#endif
		}

#if !UNITY_WEBGL || UNITY_EDITOR
		public IObservable<string> GetCovers()
		{
			IDbConnection db_connection = new SqliteConnection(db_connection_string);
			db_connection.Open();
			IDbCommand dbcmd = db_connection.CreateCommand();

			dbcmd.CommandText = "select pid_src, pid_dest FROM RELATION WHERE relation_name = 'hasCover'";
			return dbCall(dbcmd.ExecuteReader(), db_connection);
		}
#endif
		#endregion

		#region Private
		private string btoa(string toEncode)
		{
			byte[] bytes = Encoding.GetEncoding(28591).GetBytes(toEncode);
			return System.Convert.ToBase64String(bytes);
		}

		private IObservable<string> apiCall(string urlApi)
		{
			// Compute url api
			string finalUrlApi = apiBaseUrl + urlApi + identification;
#if UNITY_EDITOR
			Debug.Log("[Wezit Http Server] - apiCall - " + finalUrlApi);
#endif

			return ObservableWebRequest.Get(finalUrlApi, headers);
		}

		private IObservable<string> dbCall(IDataReader a_reader, IDbConnection a_connection, bool log = false, IProgress<float> progress = null)
		{
			return UniRx.Observable.FromCoroutine<string>((observer, cancellation) => FetchText(a_reader, a_connection, observer, progress, cancellation, log));
		}

		private IEnumerator FetchText(IDataReader a_reader, IDbConnection a_connection, IObserver<string> observer, IProgress<float> reportProgress, CancellationToken cancel, bool log = false)
		{
			StringBuilder stringBuilder = new StringBuilder();

			// Content has to be formatted into a json-compliant format
			stringBuilder.Append("{\"status\":1,\"message\":\"success\",\"data\":[");

			int fieldCount = a_reader.FieldCount;
			bool hasRead = false;
			while (a_reader.Read())
			{
				stringBuilder.Append('{');
				for (int i = 0; i < fieldCount; i++)
				{
					hasRead = true;
					stringBuilder.Append('"');
					stringBuilder.Append(a_reader.GetName(i).Replace("n.", "").Replace("NODE.", "").Replace("node.", "").Replace("ra.", ""));
					stringBuilder.Append('"');
					stringBuilder.Append(':');
					if (string.IsNullOrEmpty(a_reader.GetValue(i).ToString()))
					{
						stringBuilder.Append("null");
					}
					else
					{
						if (a_reader.GetName(i).Contains("relationList") || a_reader.GetName(i).Contains("assets") || a_reader.GetName(i).Contains("maps"))
						{
							stringBuilder.Append(a_reader.GetValue(i).ToString().Replace("<br />", "<br />\\n").Replace("\n", "").Replace("\"{", "{").Replace("}\"", "}"));
						}
						else if (typeof(float).Equals(a_reader.GetValue(i).GetType()))
						{
							stringBuilder.Append(a_reader.GetFloat(i).ToString(CultureInfo.CreateSpecificCulture("en-GB")));
						}
						else if (typeof(int).Equals(a_reader.GetValue(i).GetType()))
						{
							stringBuilder.Append(a_reader.GetInt32(i).ToString(CultureInfo.CreateSpecificCulture("en-GB")));
						}
						else
						{
							stringBuilder.Append('"');
							stringBuilder.Append(a_reader.GetValue(i).ToString().Replace("<br />", "<br />\\n").Replace("\n", "").Replace("\"", "\\\""));
							stringBuilder.Append('"');
						}
					}

					if (i < fieldCount - 1) stringBuilder.Append(",");

				}
				stringBuilder.Append("},");
				//yield return null;
			}
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
			stringBuilder.Append("]}");
			yield return null;
			if (!hasRead)
			{
				stringBuilder.Clear();
				stringBuilder.Append("{\"status\":1,\"message\":\"success / empty result\",\"data\":[]}");
			}
			a_connection.Close();
			a_connection.Dispose();

			observer.OnNext(stringBuilder.ToString());
			observer.OnCompleted();
		}
	#endregion
	#endregion
	}
}
