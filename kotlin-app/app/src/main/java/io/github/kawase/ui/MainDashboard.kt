package io.github.kawase.ui

import androidx.compose.animation.*
import androidx.compose.animation.core.*
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.*
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.blur
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.drawBehind
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.graphicsLayer
import androidx.compose.ui.graphics.luminance
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.window.Dialog
import androidx.navigation.NavController
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController

sealed class Screen(val route: String, val icon: ImageVector, val label: String) {
    object Home : Screen("home", Icons.Default.Home, "Home")
    object History : Screen("history", Icons.AutoMirrored.Filled.List, "History")
    object Goals : Screen("goals", Icons.Default.Star, "Goals")
    object Settings : Screen("settings", Icons.Default.Settings, "Settings")
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MainDashboard(viewModel: SocketViewModel) {
    val navController = rememberNavController()
    var selectedChildId by remember { mutableStateOf(-1L) }
    val children = viewModel.children
    
    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentRoute = navBackStackEntry?.destination?.route
    var showAddGoalDialog by remember { mutableStateOf(false) }

    val infiniteTransition = rememberInfiniteTransition(label = "bg")
    val rotation by infiniteTransition.animateFloat(
        initialValue = 0f, targetValue = 360f,
        animationSpec = infiniteRepeatable(tween(50000, easing = LinearEasing), RepeatMode.Restart), label = "rot"
    )

    Box(modifier = Modifier.fillMaxSize().background(MaterialTheme.colorScheme.background)) {
        // Dynamic Glass Background
        Box(modifier = Modifier.fillMaxSize().blur(100.dp).graphicsLayer(rotationZ = rotation)) {
            Box(modifier = Modifier.offset(x = (-100).dp, y = (-100).dp).size(300.dp).background(viewModel.primaryColor.value.copy(alpha = 0.15f), CircleShape))
            Box(modifier = Modifier.align(Alignment.BottomEnd).offset(x = 100.dp, y = 100.dp).size(400.dp).background(viewModel.primaryColor.value.copy(alpha = 0.1f), CircleShape))
        }

        Scaffold(
            containerColor = Color.Transparent,
            topBar = {
                CenterAlignedTopAppBar(
                    title = { 
                        Text(
                            "NEURO KEY",
                            style = MaterialTheme.typography.titleMedium,
                            fontWeight = FontWeight.Black,
                            letterSpacing = 2.sp,
                            color = MaterialTheme.colorScheme.onSurface
                        )
                    },
                    actions = {
                        // All action icons moved to Top Bar for consistency and precision
                        if (currentRoute == Screen.Home.route) {
                            IconButton(onClick = { viewModel.fetchChildren() }) {
                                Icon(Icons.Default.Refresh, contentDescription = "Refresh", tint = viewModel.primaryColor.value)
                            }
                        }
                        if (currentRoute == Screen.Goals.route && selectedChildId != -1L) {
                            IconButton(onClick = { showAddGoalDialog = true }) {
                                Icon(Icons.Default.AddCircle, contentDescription = "New Goal", tint = viewModel.primaryColor.value, modifier = Modifier.size(28.dp))
                            }
                        }
                    },
                    colors = TopAppBarDefaults.centerAlignedTopAppBarColors(
                        containerColor = Color.Transparent,
                        titleContentColor = MaterialTheme.colorScheme.onSurface
                    )
                )
            },
            bottomBar = {
                // Floating Glass Navigation
                Box(modifier = Modifier.fillMaxWidth().padding(horizontal = 24.dp, vertical = 20.dp)) {
                    Surface(
                        modifier = Modifier
                            .fillMaxWidth()
                            .height(72.dp)
                            .border(1.dp, if(viewModel.isDarkMode.value) Color.White.copy(alpha = 0.15f) else Color.Black.copy(alpha = 0.05f), CircleShape),
                        shape = CircleShape,
                        color = MaterialTheme.colorScheme.surface.copy(alpha = 0.95f),
                        tonalElevation = 8.dp,
                        shadowElevation = 12.dp
                    ) {
                        Row(
                            modifier = Modifier.fillMaxSize().padding(horizontal = 8.dp),
                            horizontalArrangement = Arrangement.SpaceAround,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            val items = listOf(Screen.Home, Screen.History, Screen.Goals, Screen.Settings)
                            items.forEach { screen ->
                                val selected = currentRoute == screen.route
                                val animatedScale by animateFloatAsState(if (selected) 1.2f else 1f, label = "scale")
                                val animatedColor by animateColorAsState(if (selected) viewModel.primaryColor.value else MaterialTheme.colorScheme.onSurface.copy(alpha = 0.5f), label = "color")

                                IconButton(
                                    onClick = {
                                        if (screen == Screen.Home) {
                                            navController.navigate(screen.route) {
                                                popUpTo(navController.graph.startDestinationId) { inclusive = true }
                                                launchSingleTop = true
                                            }
                                        } else if (screen == Screen.History || screen == Screen.Goals) {
                                            if (selectedChildId != -1L) {
                                                if (screen == Screen.History) viewModel.fetchCompletedTasks(selectedChildId)
                                                else viewModel.fetchGoals(selectedChildId)
                                                navController.navigate(screen.route) {
                                                    launchSingleTop = true
                                                    restoreState = true
                                                }
                                            }
                                        } else {
                                            navController.navigate(screen.route) {
                                                launchSingleTop = true
                                                restoreState = true
                                            }
                                        }
                                    },
                                    modifier = Modifier.size(48.dp)
                                ) {
                                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                        Icon(
                                            screen.icon, 
                                            contentDescription = screen.label,
                                            modifier = Modifier.size(24.dp).graphicsLayer(scaleX = animatedScale, scaleY = animatedScale),
                                            tint = animatedColor
                                        )
                                        if (selected) {
                                            Box(modifier = Modifier.size(4.dp).background(animatedColor, CircleShape))
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        ) { innerPadding ->
            NavHost(navController, startDestination = "home", modifier = Modifier.padding(innerPadding)) {
                composable("home") {
                    HomeScreen(viewModel, children) { childId ->
                        selectedChildId = childId
                        viewModel.fetchGoals(childId)
                        navController.navigate("goals")
                    }
                }
                composable("history") {
                    HistoryScreen(viewModel)
                }
                composable("goals") {
                    GoalsScreen(viewModel, selectedChildId)
                }
                composable("settings") {
                    SettingsScreen(viewModel)
                }
            }
        }
    }

    if (showAddGoalDialog) {
        AddGoalDialog(
            tasks = viewModel.tasks,
            isDarkMode = viewModel.isDarkMode.value,
            primaryColor = viewModel.primaryColor.value,
            onDismiss = { showAddGoalDialog = false },
            onConfirm = { title, reward, points, taskId ->
                viewModel.addGoal(selectedChildId, title, reward, points, taskId)
                showAddGoalDialog = false
            }
        )
    }
}

@Composable
fun HomeScreen(viewModel: SocketViewModel, children: List<Child>, onChildSelected: (Long) -> Unit) {
    Column(modifier = Modifier.fillMaxSize().padding(24.dp)) {
        Text(
            "My Kids",
            style = MaterialTheme.typography.headlineMedium,
            fontWeight = FontWeight.ExtraBold,
            color = MaterialTheme.colorScheme.onSurface
        )
        Text(
            "Monitor your children's progress",
            style = MaterialTheme.typography.bodyMedium,
            color = if (viewModel.isDarkMode.value) Color.White.copy(alpha = 0.8f) else Color.Black.copy(alpha = 0.6f)
        )
        
        Spacer(modifier = Modifier.height(32.dp))
        
        if (children.isEmpty()) {
            Box(modifier = Modifier.weight(1f).fillMaxWidth(), contentAlignment = Alignment.Center) {
                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    Icon(Icons.Default.Face, contentDescription = null, modifier = Modifier.size(80.dp), tint = viewModel.primaryColor.value.copy(alpha = 0.3f))
                    Spacer(modifier = Modifier.height(24.dp))
                    Text("No kids added yet", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.onSurface)
                    TextButton(onClick = { /* navigate to settings */ }) {
                        Text("Add a kid in Settings", color = viewModel.primaryColor.value, fontWeight = FontWeight.Bold)
                    }
                }
            }
        } else {
            LazyColumn(
                verticalArrangement = Arrangement.spacedBy(20.dp),
                modifier = Modifier.weight(1f)
            ) {
                items(children) { child ->
                    Box(
                        modifier = Modifier
                            .fillMaxWidth()
                            .clip(RoundedCornerShape(24.dp))
                            .background(MaterialTheme.colorScheme.surface.copy(alpha = 0.7f))
                            .border(1.dp, if (viewModel.isDarkMode.value) Color.White.copy(alpha = 0.15f) else Color.Black.copy(alpha = 0.08f), RoundedCornerShape(24.dp))
                            .clickable { onChildSelected(child.id) }
                            .padding(24.dp)
                    ) {
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            Box(contentAlignment = Alignment.Center) {
                                Box(modifier = Modifier.size(60.dp).background(viewModel.primaryColor.value.copy(alpha = 0.15f), CircleShape).border(2.dp, viewModel.primaryColor.value, CircleShape))
                                Text(
                                    child.name.take(1).uppercase(),
                                    style = MaterialTheme.typography.headlineSmall,
                                    fontWeight = FontWeight.Black,
                                    color = viewModel.primaryColor.value
                                )
                            }
                            
                            Spacer(modifier = Modifier.width(20.dp))
                            
                            Column(modifier = Modifier.weight(1f)) {
                                Text(
                                    child.name,
                                    style = MaterialTheme.typography.titleLarge,
                                    fontWeight = FontWeight.ExtraBold,
                                    color = MaterialTheme.colorScheme.onSurface
                                )
                                Spacer(modifier = Modifier.height(4.dp))
                                Row(verticalAlignment = Alignment.CenterVertically) {
                                    Icon(Icons.Default.Star, contentDescription = null, modifier = Modifier.size(16.dp), tint = viewModel.primaryColor.value)
                                    Spacer(modifier = Modifier.width(4.dp))
                                    Text(
                                        "${child.points} Points",
                                        style = MaterialTheme.typography.bodyMedium,
                                        color = if (viewModel.isDarkMode.value) Color.White.copy(alpha = 0.8f) else Color.Black.copy(alpha = 0.6f)
                                    )
                                }
                            }
                            
                            Icon(
                                Icons.AutoMirrored.Filled.KeyboardArrowRight,
                                contentDescription = null,
                                tint = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.4f)
                            )
                        }
                    }
                }
            }
        }
        Spacer(modifier = Modifier.height(100.dp))
    }
}

@Composable
fun SettingsScreen(viewModel: SocketViewModel) {
    var childName by remember { mutableStateOf("") }
    val email by viewModel.email
    val scrollState = rememberScrollState()

    Column(modifier = Modifier.fillMaxSize().padding(24.dp).verticalScroll(scrollState)) {
        Text("Settings", style = MaterialTheme.typography.headlineMedium, fontWeight = FontWeight.Black, color = MaterialTheme.colorScheme.onSurface)
        Spacer(modifier = Modifier.height(32.dp))
        
        Surface(
            modifier = Modifier.fillMaxWidth().border(1.dp, if (viewModel.isDarkMode.value) Color.White.copy(alpha = 0.15f) else Color.Black.copy(alpha = 0.08f), RoundedCornerShape(24.dp)),
            shape = RoundedCornerShape(24.dp),
            color = MaterialTheme.colorScheme.surface.copy(alpha = 0.7f)
        ) {
            Row(modifier = Modifier.padding(24.dp), verticalAlignment = Alignment.CenterVertically) {
                Surface(modifier = Modifier.size(56.dp), shape = CircleShape, color = viewModel.primaryColor.value.copy(alpha = 0.15f)) {
                    Box(contentAlignment = Alignment.Center) {
                        Icon(Icons.Default.Person, contentDescription = null, tint = viewModel.primaryColor.value)
                    }
                }
                Spacer(modifier = Modifier.width(20.dp))
                Column {
                    Text("Parent Account", style = MaterialTheme.typography.labelMedium, color = viewModel.primaryColor.value, fontWeight = FontWeight.Bold)
                    Text(email, style = MaterialTheme.typography.bodyLarge, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.onSurface)
                }
            }
        }

        Spacer(modifier = Modifier.height(32.dp))

        Text("App Theme", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.onSurface)
        Spacer(modifier = Modifier.height(16.dp))

        Surface(
            modifier = Modifier.fillMaxWidth().border(1.dp, if (viewModel.isDarkMode.value) Color.White.copy(alpha = 0.15f) else Color.Black.copy(alpha = 0.08f), RoundedCornerShape(24.dp)),
            shape = RoundedCornerShape(24.dp),
            color = MaterialTheme.colorScheme.surface.copy(alpha = 0.7f)
        ) {
            Column(modifier = Modifier.padding(24.dp)) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Text("Dark Mode", style = MaterialTheme.typography.titleSmall, modifier = Modifier.weight(1f), color = MaterialTheme.colorScheme.onSurface, fontWeight = FontWeight.Bold)
                    Switch(
                        checked = viewModel.isDarkMode.value,
                        onCheckedChange = { viewModel.setDarkMode(it) },
                        colors = SwitchDefaults.colors(
                            checkedThumbColor = Color.White,
                            checkedTrackColor = viewModel.primaryColor.value,
                            uncheckedThumbColor = Color.Gray,
                            uncheckedTrackColor = if(viewModel.isDarkMode.value) Color.DarkGray else Color.LightGray
                        )
                    )
                }
                Spacer(modifier = Modifier.height(24.dp))
                
                Text("Theme Color", style = MaterialTheme.typography.labelMedium, color = if (viewModel.isDarkMode.value) Color.White.copy(alpha = 0.8f) else Color.Black.copy(alpha = 0.5f))
                Spacer(modifier = Modifier.height(12.dp))
                Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                    listOf(Color(0xFF6366F1), Color(0xFF1B3B5F), Color(0xFFD32F2F), Color(0xFF8E24AA), Color(0xFFE64A19), Color(0xFF10B981)).forEach { color ->
                        ColorPickerOption(color, viewModel.primaryColor.value) { viewModel.setPrimaryColor(it) }
                    }
                }
            }
        }

        Spacer(modifier = Modifier.height(32.dp))
        
        Text("Manage Kids", style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.onSurface)
        Spacer(modifier = Modifier.height(16.dp))
        
        Surface(
            modifier = Modifier.fillMaxWidth().border(1.dp, if (viewModel.isDarkMode.value) Color.White.copy(alpha = 0.15f) else Color.Black.copy(alpha = 0.08f), RoundedCornerShape(24.dp)),
            shape = RoundedCornerShape(24.dp),
            color = MaterialTheme.colorScheme.surface.copy(alpha = 0.7f)
        ) {
            Column(modifier = Modifier.padding(24.dp)) {
                OutlinedTextField(
                    value = childName,
                    onValueChange = { childName = it },
                    label = { Text("Kid's Name") },
                    modifier = Modifier.fillMaxWidth(),
                    shape = RoundedCornerShape(16.dp),
                    colors = OutlinedTextFieldDefaults.colors(
                        focusedBorderColor = viewModel.primaryColor.value,
                        focusedLabelColor = viewModel.primaryColor.value,
                        unfocusedTextColor = MaterialTheme.colorScheme.onSurface,
                        focusedTextColor = MaterialTheme.colorScheme.onSurface,
                        unfocusedLabelColor = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.5f)
                    )
                )
                Spacer(modifier = Modifier.height(20.dp))
                Button(
                    onClick = { 
                        if (childName.isNotBlank()) {
                            viewModel.addChild(childName)
                            childName = ""
                        }
                    },
                    modifier = Modifier.fillMaxWidth().height(52.dp),
                    shape = RoundedCornerShape(16.dp),
                    colors = ButtonDefaults.buttonColors(containerColor = viewModel.primaryColor.value)
                ) {
                    Text("Add Kid", fontWeight = FontWeight.Bold, color = Color.White)
                }
            }
        }
        
        Spacer(modifier = Modifier.height(32.dp))
        
        TextButton(
            onClick = { viewModel.logout() },
            modifier = Modifier.fillMaxWidth(),
            colors = ButtonDefaults.textButtonColors(contentColor = MaterialTheme.colorScheme.error)
        ) {
            Icon(Icons.AutoMirrored.Filled.ExitToApp, contentDescription = null)
            Spacer(modifier = Modifier.width(8.dp))
            Text("Logout", fontWeight = FontWeight.Bold)
        }
        
        Spacer(modifier = Modifier.height(100.dp))
    }
}

@Composable
fun HistoryScreen(viewModel: SocketViewModel) {
    val history = viewModel.completedTasks

    Column(modifier = Modifier.fillMaxSize().padding(24.dp)) {
        Text("Task History", style = MaterialTheme.typography.headlineMedium, fontWeight = FontWeight.Black, color = MaterialTheme.colorScheme.onSurface)
        Text("Completed tasks from the game", style = MaterialTheme.typography.bodyMedium, color = if (viewModel.isDarkMode.value) Color.White.copy(alpha = 0.8f) else Color.Black.copy(alpha = 0.6f) )
        
        Spacer(modifier = Modifier.height(32.dp))
        
        if (history.isEmpty()) {
            Box(modifier = Modifier.weight(1f).fillMaxWidth(), contentAlignment = Alignment.Center) {
                Text("No tasks completed yet.", color = if (viewModel.isDarkMode.value) Color.White.copy(alpha = 0.4f) else Color.Black.copy(alpha = 0.3f))
            }
        } else {
            LazyColumn(verticalArrangement = Arrangement.spacedBy(16.dp)) {
                items(history) { task ->
                    Surface(
                        modifier = Modifier.fillMaxWidth().border(1.dp, if (viewModel.isDarkMode.value) Color.White.copy(alpha = 0.15f) else Color.Black.copy(alpha = 0.08f), RoundedCornerShape(20.dp)),
                        shape = RoundedCornerShape(20.dp),
                        color = MaterialTheme.colorScheme.surface.copy(alpha = 0.7f)
                    ) {
                        Row(modifier = Modifier.padding(20.dp), verticalAlignment = Alignment.CenterVertically) {
                            Surface(modifier = Modifier.size(40.dp), shape = CircleShape, color = viewModel.primaryColor.value.copy(alpha = 0.15f)) {
                                Box(contentAlignment = Alignment.Center) {
                                    Icon(Icons.Default.CheckCircle, contentDescription = null, modifier = Modifier.size(20.dp), tint = viewModel.primaryColor.value)
                                }
                            }
                            Spacer(modifier = Modifier.width(16.dp))
                            Column(modifier = Modifier.weight(1f)) {
                                Text(task.taskTitle, style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.onSurface)
                                Row(verticalAlignment = Alignment.CenterVertically) {
                                    Text("${task.pointValue} Points", style = MaterialTheme.typography.bodySmall, color = viewModel.primaryColor.value, fontWeight = FontWeight.Black)
                                    Spacer(modifier = Modifier.width(8.dp))
                                    Box(modifier = Modifier.size(2.dp).background(MaterialTheme.colorScheme.onSurface.copy(alpha = 0.3f), CircleShape))
                                    Spacer(modifier = Modifier.width(8.dp))
                                    Text(task.completedAt, style = MaterialTheme.typography.bodySmall, color = if (viewModel.isDarkMode.value) Color.White.copy(alpha = 0.8f) else Color.Black.copy(alpha = 0.5f))
                                }
                            }
                        }
                    }
                }
            }
        }
        Spacer(modifier = Modifier.height(100.dp))
    }
}

@Composable
fun GoalsScreen(viewModel: SocketViewModel, childId: Long) {
    val goals = viewModel.goals

    Column(modifier = Modifier.fillMaxSize().padding(24.dp)) {
        Text("Goals", style = MaterialTheme.typography.headlineMedium, fontWeight = FontWeight.Black, color = MaterialTheme.colorScheme.onSurface)
        Text("Set goals and rewards", style = MaterialTheme.typography.bodyMedium, color = if (viewModel.isDarkMode.value) Color.White.copy(alpha = 0.8f) else Color.Black.copy(alpha = 0.6f))
        
        Spacer(modifier = Modifier.height(32.dp))
        
        if (goals.isEmpty()) {
            Box(modifier = Modifier.weight(1f).fillMaxWidth(), contentAlignment = Alignment.Center) {
                Text("No goals set yet.", color = if (viewModel.isDarkMode.value) Color.White.copy(alpha = 0.4f) else Color.Black.copy(alpha = 0.3f))
            }
        } else {
            LazyColumn(verticalArrangement = Arrangement.spacedBy(20.dp)) {
                items(goals) { goal ->
                    Surface(
                        modifier = Modifier.fillMaxWidth().border(1.dp, if (viewModel.isDarkMode.value) Color.White.copy(alpha = 0.15f) else Color.Black.copy(alpha = 0.08f), RoundedCornerShape(24.dp)),
                        shape = RoundedCornerShape(24.dp),
                        color = MaterialTheme.colorScheme.surface.copy(alpha = 0.7f)
                    ) {
                        Row(modifier = Modifier.padding(24.dp), verticalAlignment = Alignment.CenterVertically) {
                            Column(modifier = Modifier.weight(1f)) {
                                Text(
                                    goal.title,
                                    style = MaterialTheme.typography.titleMedium,
                                    fontWeight = FontWeight.ExtraBold,
                                    color = if (goal.completed) viewModel.primaryColor.value else MaterialTheme.colorScheme.onSurface
                                )
                                Spacer(modifier = Modifier.height(4.dp))
                                Row(verticalAlignment = Alignment.CenterVertically) {
                                    Icon(Icons.Default.ShoppingCart, contentDescription = null, modifier = Modifier.size(14.dp), tint = viewModel.primaryColor.value)
                                    Spacer(modifier = Modifier.width(6.dp))
                                    Text("Reward: ${goal.reward}", style = MaterialTheme.typography.bodySmall, color = if (viewModel.isDarkMode.value) Color.White.copy(alpha = 0.8f) else Color.Black.copy(alpha = 0.6f))
                                }
                            }
                            
                            Surface(
                                shape = CircleShape,
                                color = if (goal.completed) viewModel.primaryColor.value else Color.Transparent,
                                border = if (goal.completed) null else androidx.compose.foundation.BorderStroke(2.dp, MaterialTheme.colorScheme.onSurface.copy(alpha = 0.1f))
                            ) {
                                Icon(
                                    if (goal.completed) Icons.Default.Check else Icons.Default.Lock, 
                                    contentDescription = null, 
                                    modifier = Modifier.padding(8.dp).size(20.dp), 
                                    tint = if (goal.completed) Color.White else MaterialTheme.colorScheme.onSurface.copy(alpha = 0.3f)
                                )
                            }
                        }
                    }
                }
            }
        }
        Spacer(modifier = Modifier.height(100.dp))
    }
}

@Composable
fun ColorPickerOption(color: Color, selectedColor: Color, onColorSelected: (Color) -> Unit) {
    Box(
        modifier = Modifier
            .size(44.dp)
            .clip(RoundedCornerShape(12.dp))
            .background(color)
            .clickable { onColorSelected(color) }
            .border(
                if (color == selectedColor) 3.dp else 0.dp, 
                if (color == selectedColor) (if(selectedColor.luminance() < 0.3f) Color.White else Color.Black) else Color.Transparent, 
                RoundedCornerShape(12.dp)
            )
    )
}

@Composable
fun AddGoalDialog(
    tasks: List<Task>,
    isDarkMode: Boolean,
    primaryColor: Color,
    onDismiss: () -> Unit,
    onConfirm: (String, String, Int, Long) -> Unit
) {
    var title by remember { mutableStateOf("") }
    var reward by remember { mutableStateOf("") }
    var points by remember { mutableStateOf("50") }
    var selectedTaskId by remember { mutableStateOf(-1L) }
    var usePoints by remember { mutableStateOf(true) }

    Dialog(onDismissRequest = onDismiss) {
        Surface(
            modifier = Modifier
                .fillMaxWidth()
                .clip(RoundedCornerShape(32.dp))
                .border(1.dp, if(isDarkMode) Color.White.copy(alpha = 0.15f) else Color.Black.copy(alpha = 0.08f), RoundedCornerShape(32.dp)),
            color = if(isDarkMode) Color(0xFF1A1A1A).copy(alpha = 0.95f) else MaterialTheme.colorScheme.surface.copy(alpha = 0.95f),
            tonalElevation = 8.dp
        ) {
            Column(
                modifier = Modifier.padding(32.dp),
                verticalArrangement = Arrangement.spacedBy(16.dp)
            ) {
                Text(
                    "New Goal", 
                    style = MaterialTheme.typography.headlineSmall, 
                    fontWeight = FontWeight.Black, 
                    color = MaterialTheme.colorScheme.onSurface
                )
                
                Spacer(modifier = Modifier.height(8.dp))

                OutlinedTextField(
                    value = title,
                    onValueChange = { title = it },
                    label = { Text("Goal Name") },
                    modifier = Modifier.fillMaxWidth(),
                    shape = RoundedCornerShape(16.dp),
                    colors = OutlinedTextFieldDefaults.colors(
                        focusedBorderColor = primaryColor,
                        focusedLabelColor = primaryColor,
                        unfocusedTextColor = MaterialTheme.colorScheme.onSurface,
                        focusedTextColor = MaterialTheme.colorScheme.onSurface,
                        unfocusedContainerColor = if(isDarkMode) Color.Black.copy(alpha = 0.3f) else Color.Black.copy(alpha = 0.03f),
                        focusedContainerColor = if(isDarkMode) Color.Black.copy(alpha = 0.4f) else Color.Black.copy(alpha = 0.05f)
                    )
                )
                OutlinedTextField(
                    value = reward,
                    onValueChange = { reward = it },
                    label = { Text("Reward") },
                    modifier = Modifier.fillMaxWidth(),
                    shape = RoundedCornerShape(16.dp),
                    colors = OutlinedTextFieldDefaults.colors(
                        focusedBorderColor = primaryColor,
                        focusedLabelColor = primaryColor,
                        unfocusedTextColor = MaterialTheme.colorScheme.onSurface,
                        focusedTextColor = MaterialTheme.colorScheme.onSurface,
                        unfocusedContainerColor = if(isDarkMode) Color.Black.copy(alpha = 0.3f) else Color.Black.copy(alpha = 0.03f),
                        focusedContainerColor = if(isDarkMode) Color.Black.copy(alpha = 0.4f) else Color.Black.copy(alpha = 0.05f)
                    )
                )
                
                HorizontalDivider(modifier = Modifier.padding(vertical = 8.dp), color = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.1f))
                
                Text("How to complete", style = MaterialTheme.typography.labelMedium, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.onSurface)
                Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                    FilterChip(
                        selected = usePoints,
                        onClick = { usePoints = true },
                        label = { Text("Points") },
                        shape = RoundedCornerShape(12.dp),
                        modifier = Modifier.weight(1f)
                    )
                    FilterChip(
                        selected = !usePoints,
                        onClick = { usePoints = false },
                        label = { Text("Static Task") },
                        shape = RoundedCornerShape(12.dp),
                        modifier = Modifier.weight(1f)
                    )
                }

                if (usePoints) {
                    OutlinedTextField(
                        value = points,
                        onValueChange = { points = it },
                        label = { Text("Points Required") },
                        modifier = Modifier.fillMaxWidth(),
                        shape = RoundedCornerShape(16.dp),
                        colors = OutlinedTextFieldDefaults.colors(
                            focusedBorderColor = primaryColor,
                            focusedLabelColor = primaryColor,
                            unfocusedTextColor = MaterialTheme.colorScheme.onSurface,
                            focusedTextColor = MaterialTheme.colorScheme.onSurface,
                            unfocusedContainerColor = if(isDarkMode) Color.Black.copy(alpha = 0.3f) else Color.Black.copy(alpha = 0.03f),
                            focusedContainerColor = if(isDarkMode) Color.Black.copy(alpha = 0.4f) else Color.Black.copy(alpha = 0.05f)
                        )
                    )
                } else {
                    Text("Select Task:", style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.onSurface)
                    LazyColumn(modifier = Modifier.heightIn(max = 200.dp)) {
                        items(tasks) { task ->
                            Surface(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .clickable { selectedTaskId = task.id }
                                    .padding(vertical = 4.dp),
                                shape = RoundedCornerShape(12.dp),
                                color = if (selectedTaskId == task.id) primaryColor.copy(alpha = 0.2f) else Color.Transparent,
                                border = androidx.compose.foundation.BorderStroke(1.dp, if (selectedTaskId == task.id) primaryColor else MaterialTheme.colorScheme.onSurface.copy(alpha = 0.1f))
                            ) {
                                Row(modifier = Modifier.padding(16.dp), verticalAlignment = Alignment.CenterVertically) {
                                    RadioButton(selected = selectedTaskId == task.id, onClick = { selectedTaskId = task.id })
                                    Spacer(modifier = Modifier.width(12.dp))
                                    Text(task.name, style = MaterialTheme.typography.bodyMedium, fontWeight = if(selectedTaskId == task.id) FontWeight.Bold else FontWeight.Normal, color = MaterialTheme.colorScheme.onSurface)
                                }
                            }
                        }
                    }
                }

                Spacer(modifier = Modifier.height(16.dp))

                Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.End) {
                    TextButton(onClick = onDismiss) {
                        Text("Cancel")
                    }
                    Spacer(modifier = Modifier.width(8.dp))
                    Button(
                        onClick = { onConfirm(title, reward, points.toIntOrNull() ?: 0, if (usePoints) -1L else selectedTaskId) },
                        enabled = title.isNotBlank() && reward.isNotBlank(),
                        shape = RoundedCornerShape(12.dp),
                        colors = ButtonDefaults.buttonColors(containerColor = primaryColor)
                    ) {
                        Text("Save Goal", fontWeight = FontWeight.Bold, color = Color.White)
                    }
                }
            }
        }
    }
}
