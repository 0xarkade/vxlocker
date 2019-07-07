<?PHP
//error_reporting(0);

$user = "root";
$pass = "test123qwe";
$db = "vxcontrol";
	
$con = mysql_connect("localhost", $user, $pass);
	
if($con)
{
  mysql_select_db($db);
}
else die();
?>