package io.github.kawase.ui

import androidx.compose.runtime.State
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import io.github.kawase.socket.packet.Packet
import io.github.kawase.socket.packet.PacketManager
import io.github.kawase.socket.packet.impl.*
import io.github.kawase.socket.utility.HashUtility
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.launch
import org.java_websocket.client.WebSocketClient
import org.java_websocket.handshake.ServerHandshake
import java.net.URI
import java.nio.ByteBuffer

import androidx.compose.ui.graphics.Color
import io.github.kawase.ui.theme.PrimaryLight
import io.github.kawase.ui.theme.SecondaryLight
import io.github.kawase.ui.theme.BackgroundWhite
import io.github.kawase.ui.theme.SurfaceGray

import android.app.Application
import android.content.Context
import android.content.SharedPreferences
import androidx.compose.ui.graphics.toArgb
import androidx.lifecycle.AndroidViewModel

data class Child(val id: Long, val name: String, val points: Int)
data class Task(val id: Long, val name: String, val points: Int)
data class Goal(val id: Long, val title: String, val reward: String, val completed: Boolean, val requiredPoints: Int)
data class CompletedTask(val id: Long, val taskTitle: String, val pointValue: Int, val completedAt: String)

class SocketViewModel(application: Application) : AndroidViewModel(application) {
    private var client: AndroidClientSocket? = null
    private val packetManager = PacketManager()
    private val prefs: SharedPreferences = application.getSharedPreferences("neurokey_prefs", Context.MODE_PRIVATE)

    var isDarkMode = mutableStateOf(prefs.getBoolean("dark_mode", false))
    var primaryColor = mutableStateOf(Color(prefs.getInt("primary_color", PrimaryLight.toArgb())))
    var secondaryColor = mutableStateOf(Color(prefs.getInt("secondary_color", SecondaryLight.toArgb())))

    private val _isConnected = mutableStateOf(false)
    val isConnected: State<Boolean> = _isConnected

    private val _isLoggedIn = mutableStateOf(false)
    val isLoggedIn: State<Boolean> = _isLoggedIn

    private val _parentId = mutableStateOf(-1L)
    val parentId: State<Long> = _parentId

    private val _email = mutableStateOf(prefs.getString("saved_email", "") ?: "")
    val email: State<String> = _email
    
    private var savedEmailHash: String? = prefs.getString("email_hash", null)
    private var savedPasswordHash: String? = prefs.getString("password_hash", null)

    fun toggleDarkMode() {
        isDarkMode.value = !isDarkMode.value
        prefs.edit().putBoolean("dark_mode", isDarkMode.value).apply()
    }

    fun updatePrimaryColor(color: Color) {
        primaryColor.value = color
        prefs.edit().putInt("primary_color", color.toArgb()).apply()
    }

    fun setSecondaryColor(color: Color) {
        secondaryColor.value = color
        prefs.edit().putInt("secondary_color", color.toArgb()).apply()
    }

    fun logout() {
        _isLoggedIn.value = false
        savedEmailHash = null
        savedPasswordHash = null
        prefs.edit()
            .remove("saved_email")
            .remove("email_hash")
            .remove("password_hash")
            .apply()
    }

    private val _children = mutableStateListOf<Child>()
    val children: List<Child> = _children

    private val _tasks = mutableStateListOf<Task>()
    val tasks: List<Task> = _tasks

    private val _goals = mutableStateListOf<Goal>()
    val goals: List<Goal> = _goals

    private val _completedTasks = mutableStateListOf<CompletedTask>()
    val completedTasks: List<CompletedTask> = _completedTasks

    private val _errorFlow = MutableSharedFlow<String>()
    val errorFlow: SharedFlow<String> = _errorFlow.asSharedFlow()

    private val _successFlow = MutableSharedFlow<String>()
    val successFlow: SharedFlow<String> = _successFlow.asSharedFlow()

    fun connect(url: String = "ws://10.0.2.2:8887") {
        viewModelScope.launch(Dispatchers.IO) {
            try {
                if (client?.isOpen == true) return@launch
                client = AndroidClientSocket(URI(url))
                client?.connect()
            } catch (e: Exception) {
                _errorFlow.emit("Connection failed: ${e.message}")
            }
        }
    }

    fun login(email: String, password: String) {
        this._email.value = email
        val emailHash = HashUtility.hash(email)
        val passwordHash = HashUtility.hash(password)
        
        savedEmailHash = emailHash
        savedPasswordHash = passwordHash
        prefs.edit()
            .putString("saved_email", email)
            .putString("email_hash", emailHash)
            .putString("password_hash", passwordHash)
            .apply()
            
        sendPacket(AuthPacket(emailHash, passwordHash))
    }

    fun register(email: String, password: String) {
        this._email.value = email
        val emailHash = HashUtility.hash(email)
        val passwordHash = HashUtility.hash(password)
        
        savedEmailHash = emailHash
        savedPasswordHash = passwordHash
        prefs.edit()
            .putString("saved_email", email)
            .putString("email_hash", emailHash)
            .putString("password_hash", passwordHash)
            .apply()
            
        sendPacket(RegisterParentPacket(emailHash, passwordHash))
    }

    fun addChild(name: String) {
        sendPacket(AddChildPacket(name))
    }

    fun fetchChildren() {
        sendPacket(FetchChildrenPacket())
    }

    fun fetchTasks() {
        sendPacket(FetchTasksPacket())
    }

    fun fetchGoals(childId: Long) {
        sendPacket(FetchGoalsPacket(childId))
    }

    fun fetchCompletedTasks(childId: Long) {
        sendPacket(FetchCompletedTasksPacket(childId))
    }

    fun addGoal(childId: Long, title: String, reward: String, points: Int, taskId: Long) {
        sendPacket(AddGoalPacket(childId, title, reward, points, taskId))
    }

    fun claimQRLogin(token: String, childId: Long) {
        sendPacket(ClaimQRLoginPacket(token, childId))
    }

    private fun sendPacket(packet: Packet) {
        viewModelScope.launch(Dispatchers.IO) {
            client?.let {
                if (it.isOpen) {
                    it.send(packet.encode())
                } else {
                    _errorFlow.emit("Not connected to server")
                }
            } ?: run {
                _errorFlow.emit("Not connected to server")
            }
        }
    }

    private inner class AndroidClientSocket(uri: URI) : WebSocketClient(uri) {
        override fun onOpen(handshakedata: ServerHandshake?) {
            _isConnected.value = true
            send(HandShakePacket("android_client").encode())
            
            if (savedEmailHash != null && savedPasswordHash != null) {
                send(AuthPacket(savedEmailHash, savedPasswordHash).encode())
            }
        }

        override fun onMessage(message: String?) {}

        override fun onMessage(bytes: ByteBuffer?) {
            bytes?.let {
                try {
                    val packet = Packet.construct(it, packetManager)
                    handlePacket(packet)
                } catch (e: Exception) {
                    viewModelScope.launch { _errorFlow.emit("Packet error: ${e.message}") }
                }
            }
        }

        override fun onClose(code: Int, reason: String?, remote: Boolean) {
            _isConnected.value = false
            _isLoggedIn.value = false
        }

        override fun onError(ex: Exception?) {
            viewModelScope.launch { _errorFlow.emit("Socket error: ${ex?.message}") }
        }
    }

    private fun handlePacket(packet: Packet) {
        when (packet) {
            is AuthResponsePacket -> {
                if (packet.isSuccess) {
                    _parentId.value = packet.parentId
                    _isLoggedIn.value = true
                    viewModelScope.launch { _successFlow.emit("Logged in successfully") }
                    fetchChildren()
                    fetchTasks()
                } else {
                    viewModelScope.launch { _errorFlow.emit("Auth failed: ${packet.message}") }
                }
            }
            is ActionResponsePacket -> {
                viewModelScope.launch {
                    if (packet.isSuccess) {
                        _successFlow.emit(packet.message ?: "Action successful")
                        if (packet.requestPacketId == 4) { // AddChild
                            fetchChildren()
                        }
                    } else {
                        _errorFlow.emit("Action failed: ${packet.message}")
                    }
                }
            }
            is FetchChildrenResponsePacket -> {
                _children.clear()
                packet.children.forEach { child ->
                    _children.add(Child(child.id, child.name, child.totalPoints))
                }
            }
            is FetchTasksResponsePacket -> {
                _tasks.clear()
                packet.tasks.forEach { task ->
                   _tasks.add(Task(task.id, task.title, task.pointValue))
                }
            }
            is FetchGoalsResponsePacket -> {
                _goals.clear()
                packet.goals.forEach { goal ->
                    _goals.add(Goal(goal.id, goal.title, goal.reward, goal.isCompleted, goal.requiredPoints))
                }
            }
            is FetchCompletedTasksResponsePacket -> {
                _completedTasks.clear()
                packet.completedTasks.forEach { ct ->
                    _completedTasks.add(CompletedTask(ct.id, ct.taskTitle, ct.pointValue, ct.completedAt))
                }
            }
        }
    }
}
