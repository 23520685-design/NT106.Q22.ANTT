// ============================================================
// PROFILE JS
// Dùng Font Awesome giống Home
// Có WebView2 communication giống Home nhưng tách riêng cho Profile
// ============================================================

function profileSendToCSharp(message) {
  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.postMessage(message);
  } else {
    console.log("[PROFILE DEV] WebView2 not available:", message);
  }
}

function profileEscapeHTML(value) {
  return String(value || "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}

function profileFormatTime(ts) {
  if (!ts) return "Vừa xong";

  let date = null;

  if (typeof ts === "number") {
    date = new Date(ts * 1000);
  } else if (typeof ts === "string") {
    date = new Date(ts);
  } else if (typeof ts === "object") {
    if (ts.seconds != null) {
      date = new Date(ts.seconds * 1000);
    } else if (ts._seconds != null) {
      date = new Date(ts._seconds * 1000);
    }
  }

  if (!date || isNaN(date.getTime())) return "Vừa xong";

  return date.toLocaleString("vi-VN");
}

let profileSelectedImagePath = null;
let profileCurrentUser = {
  userId: "",
  userName: "",
  avatar: "",
  bio: "",
};


document.addEventListener("DOMContentLoaded", function () {
  initProfileSidebar();
  initProfileTabs();
  initProfilePostModal();
  initProfileLikeButtons();
  initProfileCommentButtons();
  initProfileSearch();
  initProfileWebViewMessages();
  initProfileMenu();
  initEditProfileModal();

  renderProfileUser(profileCurrentUser);

});

// ============================================================
// SIDEBAR
// ============================================================

function initProfileSidebar() {
  const leftSidebar = document.getElementById("profileSidebarLeft");
  const rightSidebar = document.getElementById("profileSidebarRight");

  const toggleLeft = document.getElementById("toggleLeftSidebar");
  const toggleRight = document.getElementById("toggleRightSidebar");

  if (toggleLeft && leftSidebar) {
    toggleLeft.addEventListener("click", function () {
      leftSidebar.classList.toggle("collapsed");
    });
  }

  if (toggleRight && rightSidebar) {
    toggleRight.addEventListener("click", function () {
      rightSidebar.classList.toggle("collapsed");
    });
  }

  document.querySelectorAll(".profile-mini-icons i").forEach(function (icon) {
    icon.addEventListener("click", function () {
      const page = icon.dataset.page;

      document.querySelectorAll(".profile-mini-icons i").forEach(function (item) {
        item.classList.remove("active");
      });

      icon.classList.add("active");

      document.querySelectorAll(".profile-menu-item").forEach(function (item) {
        item.classList.remove("active");
      });

      const linkedMenu = document.querySelector(
        '.profile-menu-link[data-page="' + page + '"]',
      );

      if (linkedMenu && linkedMenu.parentElement) {
        linkedMenu.parentElement.classList.add("active");
      }

      handleProfileMenuPage(page);
    });
  });
}

// ============================================================
// MENU
// ============================================================

function initProfileMenu() {
  const menuLinks = document.querySelectorAll(".profile-menu-link");

  menuLinks.forEach(function (link) {
    link.addEventListener("click", function (event) {
      event.preventDefault();

      const page = link.dataset.page;

      document.querySelectorAll(".profile-menu-item").forEach(function (item) {
        item.classList.remove("active");
      });

      if (link.parentElement) {
        link.parentElement.classList.add("active");
      }

      handleProfileMenuPage(page);
    });
  });
}

function handleProfileMenuPage(page) {
  if (page === "home") {
    profileSendToCSharp({ type: "NAVIGATE_HOME" });
    showProfileToast("Chuyển về trang chủ");
    return;
  }

  if (page === "create-post") {
    if (!isMyProfile()) {
      showProfileToast("Bạn chỉ có thể đăng bài trên trang cá nhân của mình");
      return;
    }

    openProfilePostModal();
    return;
  }

  if (page === "logout") {
    const ok = confirm("Bạn có chắc muốn đăng xuất?");
    if (ok) {
      profileSendToCSharp({ type: "LOGOUT" });
    }
    return;
  }

  if (page === "profile") {
    profileSendToCSharp({ type: "NAVIGATE_MY_PROFILE" });
    showProfileToast("Chuyển về trang cá nhân");
    return;
  }

  showProfileToast("Chức năng đang được phát triển");
}

// ============================================================
// TABS
// ============================================================

function initProfileTabs() {
  const tabs = document.querySelectorAll(".profile-tab");

  tabs.forEach(function (tab) {
    tab.addEventListener("click", function () {
      tabs.forEach(function (item) {
        item.classList.remove("active");
      });

      tab.classList.add("active");

      const tabName = tab.dataset.tab;
      renderProfileTab(tabName);
    });
  });
}

function renderProfileTab(tabName) {
  const tabContent = document.getElementById("profileTabContent");

  if (!tabContent) return;

  if (tabName === "posts") {
    tabContent.innerHTML = `
    <div id="profilePostsContainer" class="profile-posts-container">
      <div class="profile-empty-state">
        <p>Đang tải bài viết...</p>
      </div>
    </div>
  `;

    if (profileViewingUserId) {
      profileSendToCSharp({
        type: "GET_USER_PROFILE",
        data: { userId: profileViewingUserId }
      });
    }

    return;
  }

  let title = "";
  let desc = "";

  if (tabName === "about") {
    title = "Giới thiệu";
    desc =
      "Thông tin cá nhân, trường học, công việc và mô tả người dùng sẽ hiển thị tại đây.";
  }

  if (tabName === "friends") {
    title = "Bạn bè";
    desc = "Danh sách bạn bè của người dùng sẽ hiển thị tại đây.";
  }

  if (tabName === "photos") {
    title = "Ảnh";
    desc = "Tất cả ảnh người dùng đã đăng sẽ hiển thị tại đây.";
  }

  if (tabName === "videos") {
    title = "Video";
    desc = "Tất cả video người dùng đã đăng sẽ hiển thị tại đây.";
  }

  tabContent.innerHTML = `
    <div class="profile-empty-state">
      <h3>${profileEscapeHTML(title)}</h3>
      <p>${profileEscapeHTML(desc)}</p>
    </div>
  `;
}

// ============================================================
// MODAL CREATE POST
// ============================================================

function initProfilePostModal() {
  const openBtn = document.getElementById("openCreatePostModal");
  const closeBtn = document.getElementById("closeCreatePostModal");
  const cancelBtn = document.getElementById("cancelCreatePost");
  const publishBtn = document.getElementById("btnPublishProfilePost");
  const modal = document.getElementById("profilePostModal");

  const quickImage = document.getElementById("btnQuickImage");
  const chooseImage = document.getElementById("btnChooseProfileImage");

  if (openBtn) {
    openBtn.addEventListener("click", function () {
      if (!isMyProfile()) {
        showProfileToast("Bạn chỉ có thể đăng bài trên trang cá nhân của mình");
        return;
      }

      openProfilePostModal();
    });
  }

  if (closeBtn) {
    closeBtn.addEventListener("click", closeProfilePostModal);
  }

  if (cancelBtn) {
    cancelBtn.addEventListener("click", closeProfilePostModal);
  }

  if (modal) {
    modal.addEventListener("click", function (event) {
      if (event.target === modal) {
        closeProfilePostModal();
      }
    });
  }

  if (publishBtn) {
    publishBtn.addEventListener("click", publishProfilePost);
  }

  if (quickImage) {
    quickImage.addEventListener("click", function () {
      if (!isMyProfile()) {
        showProfileToast("Bạn chỉ có thể đăng bài trên trang cá nhân của mình");
        return;
      }

      openProfilePostModal();
      profileSendToCSharp({ type: "CHOOSE_IMAGE" });
    });
  }

  if (chooseImage) {
    chooseImage.addEventListener("click", function () {
      profileSendToCSharp({ type: "CHOOSE_IMAGE" });
    });
  }

  document.addEventListener("keydown", function (event) {
    if (event.key === "Escape") {
      closeProfilePostModal();
    }
  });
}

function openProfilePostModal() {
  const modal = document.getElementById("profilePostModal");
  const input = document.getElementById("profilePostContent");

  if (!modal) return;

  modal.classList.remove("hidden");
  document.body.style.overflow = "hidden";

  setTimeout(function () {
    if (input) input.focus();
  }, 100);
}

function closeProfilePostModal() {
  const modal = document.getElementById("profilePostModal");

  if (!modal) return;

  modal.classList.add("hidden");
  document.body.style.overflow = "";
}

function publishProfilePost() {
  if (!isMyProfile()) {
    showProfileToast("Bạn chỉ có thể đăng bài trên trang cá nhân của mình");
    return;
  }

  const input = document.getElementById("profilePostContent");
  const visibility = document.getElementById("profilePostVisibility");
  const publishBtn = document.getElementById("btnPublishProfilePost");

  if (!input) return;

  const content = input.value.trim();

  if (!content && !profileSelectedImagePath) {
    input.focus();
    showProfileToast("Vui lòng nhập nội dung bài viết");
    return;
  }

  if (publishBtn) {
    publishBtn.disabled = true;
    publishBtn.innerHTML =
      '<i class="fas fa-spinner fa-spin"></i> Đang đăng...';
  }

  profileSendToCSharp({
    type: "CREATE_POST",
    data: {
      content: content,
      imagePath: profileSelectedImagePath,
      visibility: visibility ? visibility.value : "public",
    },
  });
}

// ============================================================
// PROFILE USER
// ============================================================

function renderProfileUser(user) {
  profileCurrentUser = {
    ...profileCurrentUser,
    ...user,
  };

  const name = profileCurrentUser.userName || "Người dùng";
  const avatar =
    profileCurrentUser.avatar || "https://i.pravatar.cc/150?img=11";
  const bio = profileCurrentUser.bio || "";

  setText("profileUserName", name);
  setText("profileUserBio", bio);
  setText("sidebarUserName", name);
  setText("modalAuthorName", name);

  setImage("profileAvatar", avatar);
  setImage("modalAuthorAvatar", avatar);
  setImage("sidebarUserAvatar", avatar);
  setImage("topUserAvatar", avatar);
}

function setText(id, value) {
  const el = document.getElementById(id);
  if (el) el.textContent = value;
}

function setImage(id, src) {
  const el = document.getElementById(id);
  if (el && src) el.src = src;
}

// ============================================================
// POSTS
// ============================================================

function getDemoProfilePosts() {
  return [
    {
      postId: "demo_1",
      userName: profileCurrentUser.userName,
      avatar: profileCurrentUser.avatar,
      content:
        "Đây là giao diện Profile được viết riêng, đồng bộ màu với Home nhưng không import trực tiếp từ Home.",
      createdAt: new Date().toISOString(),
      visibility: "public",
      likeCount: 12,
      commentCount: 5,
      isLiked: false,
      hasDemoImage: true,
    },
    {
      postId: "demo_2",
      userName: profileCurrentUser.userName,
      avatar: profileCurrentUser.avatar,
      content:
        "Layout Profile gồm menu bên trái, phần hồ sơ và bài viết ở giữa, thống kê và bạn bè hoạt động bên phải.",
      createdAt: new Date().toISOString(),
      visibility: "followers",
      likeCount: 28,
      commentCount: 9,
      isLiked: false,
      hasDemoImage: false,
    },
  ];
}

function renderProfilePosts(posts) {
  const container = document.getElementById("profilePostsContainer");
  if (!container) return;

  if (!posts || posts.length === 0) {
    container.innerHTML = `
      <div class="profile-empty-state">
        <h3>Chưa có bài viết</h3>
        <p>Người dùng này chưa đăng bài viết nào.</p>
      </div>
    `;
    return;
  }

  let html = "";

  posts.forEach(function (post) {
    html += buildProfilePostHTML(post);
  });

  container.innerHTML = html;
  initProfileLikeButtons();
  initProfileCommentButtons();
}

function buildProfilePostHTML(post) {
  const likedClass = post.isLiked ? "liked" : "";
  const visibilityText = getVisibilityText(post.visibility);

  return `
    <article class="profile-post-card profile-glass" data-post-id="${profileEscapeHTML(post.postId)}">
      <div class="profile-post-header">
        <div class="profile-post-author">
          <img
            src="${profileEscapeHTML(post.avatar || "https://i.pravatar.cc/150")}"
            alt="${profileEscapeHTML(post.userName || "User")}"
            onerror="this.src='https://i.pravatar.cc/150'"
          />
          <div>
            <h4>${profileEscapeHTML(post.userName || "Ẩn danh")}</h4>
            <span>${profileFormatTime(post.createdAt)} · ${profileEscapeHTML(visibilityText)}</span>
          </div>
        </div>

        <button class="profile-more-btn"><i class="fas fa-ellipsis-h"></i></button>
      </div>

      <div class="profile-post-content">
        <p>${profileEscapeHTML(post.content || "")}</p>
      </div>

      ${post.mediaUrl
      ? `<div class="profile-post-real-image"><img src="${profileEscapeHTML(post.mediaUrl)}" alt="Ảnh bài viết" /></div>`
      : post.hasDemoImage
        ? `<div class="profile-post-image"></div>`
        : ""
    }

      <div class="profile-post-stats">
        <span>${post.likeCount || 0} lượt thích</span>
        <span>${post.commentCount || 0} bình luận</span>
      </div>

      <div class="profile-post-actions">
        <button class="profile-like-btn ${likedClass}"><i class="${post.isLiked ? "fas" : "far"} fa-thumbs-up"></i><span>${post.isLiked ? "Đã thích" : "Thích"}</span></button>
        <button class="profile-comment-btn"><i class="far fa-comment-alt"></i><span>Bình luận</span></button>
        <button class="profile-share-btn"><i class="far fa-share-square"></i><span>Chia sẻ</span></button>
      </div>
    </article>
  `;
}

function createProfilePostLocal(post) {
  const container = document.getElementById("profilePostsContainer");

  if (!container) return;

  const empty = container.querySelector(".profile-empty-state");
  if (empty) {
    container.innerHTML = "";
  }

  container.insertAdjacentHTML("afterbegin", buildProfilePostHTML(post));
  initProfileLikeButtons();
  initProfileCommentButtons();
}

function getVisibilityText(value) {
  if (value === "private") return "Riêng tư";
  if (value === "followers") return "Chỉ follower";
  return "Công khai";
}

// ============================================================
// LIKE
// ============================================================

function initProfileLikeButtons() {
  const likeButtons = document.querySelectorAll(".profile-like-btn");

  likeButtons.forEach(function (button) {
    button.onclick = function () {
      // Debounce: tránh spam click trước khi server phản hồi
      if (button.dataset.loading === "true") return;
      button.dataset.loading = "true";

      const post = button.closest(".profile-post-card");
      const postId = post ? post.dataset.postId : null;

      // Optimistic UI toggle
      button.classList.toggle("liked");

      const label = button.querySelector("span");
      const icon = button.querySelector("i");

      if (button.classList.contains("liked")) {
        if (label) label.textContent = "Đã thích";
        if (icon) {
          icon.classList.remove("far");
          icon.classList.add("fas");
        }
      } else {
        if (label) label.textContent = "Thích";
        if (icon) {
          icon.classList.remove("fas");
          icon.classList.add("far");
        }
      }

      if (postId) {
        profileSendToCSharp({
          type: "TOGGLE_LIKE",
          data: { postId: postId },
        });
      }

      // Mở khóa sau 500ms (đủ thời gian server phản hồi)
      setTimeout(function () {
        button.dataset.loading = "false";
      }, 500);
    };
  });
}

// ============================================================
// COMMENT BUTTONS IN PROFILE
// ============================================================
function initProfileCommentButtons() {
  const commentButtons = document.querySelectorAll(".profile-comment-btn");

  commentButtons.forEach(function (button) {
    button.onclick = function (e) {
      e.stopPropagation();

      const post = button.closest(".profile-post-card");
      const postId = post ? post.dataset.postId : null;
      if (!postId) return;

      // Mở modal bình luận giống ở Home
      openProfileCommentModal(postId, post);
    };
  });
}

function openProfileCommentModal(postId, postEl) {
  // Tái sử dụng modal bình luận đơn giản
  const existingModal = document.getElementById("profileCommentModal");
  if (existingModal) existingModal.remove();

  const avatarSrc =
    profileCurrentUser && profileCurrentUser.avatar
      ? profileCurrentUser.avatar
      : "https://i.pravatar.cc/100?u=current";

  const postContent = postEl ? postEl.querySelector(".profile-post-content p") : null;
  const previewText = postContent ? postContent.textContent.substring(0, 80) : "";

  const modal = document.createElement("div");
  modal.id = "profileCommentModal";
  modal.style.cssText = [
    "position:fixed", "inset:0", "background:rgba(0,0,0,0.6)",
    "display:flex", "align-items:center", "justify-content:center",
    "z-index:9999", "padding:20px"
  ].join(";");

  modal.innerHTML = `
    <div style="background:rgba(30,30,50,0.97);border-radius:16px;padding:24px;width:100%;max-width:500px;max-height:80vh;display:flex;flex-direction:column;gap:16px;box-shadow:0 8px 32px rgba(0,0,0,0.5);">
      <div style="display:flex;justify-content:space-between;align-items:center;">
        <h3 style="margin:0;color:#e2e8f0;font-size:16px;"><i class="fas fa-comment-alt" style="margin-right:8px;color:#6366f1;"></i>Bình luận</h3>
        <button id="closeProfileCommentModal" style="background:none;border:none;color:#94a3b8;font-size:18px;cursor:pointer;"><i class="fas fa-times"></i></button>
      </div>
      ${previewText ? `<p style="margin:0;color:#94a3b8;font-size:13px;border-left:3px solid #6366f1;padding-left:10px;">${profileEscapeHTML(previewText)}${previewText.length >= 80 ? "..." : ""}</p>` : ""}
      <div id="profileCommentList" style="flex:1;overflow-y:auto;max-height:280px;display:flex;flex-direction:column;gap:10px;">
        <div style="color:#94a3b8;font-size:13px;text-align:center;padding:20px 0;">Đang tải bình luận...</div>
      </div>
      <div style="display:flex;gap:10px;align-items:flex-start;">
        <img src="${profileEscapeHTML(avatarSrc)}" style="width:36px;height:36px;border-radius:50%;object-fit:cover;flex-shrink:0;" onerror="this.src='https://i.pravatar.cc/100'" />
        <div style="flex:1;display:flex;flex-direction:column;gap:8px;">
          <textarea id="profileCommentInput" placeholder="Viết bình luận..." rows="2" style="width:100%;background:rgba(255,255,255,0.08);border:1px solid rgba(255,255,255,0.15);border-radius:10px;padding:10px;color:#e2e8f0;font-size:14px;resize:none;font-family:inherit;box-sizing:border-box;"></textarea>
          <button id="btnSendProfileComment" style="align-self:flex-end;background:linear-gradient(135deg,#6366f1,#8b5cf6);border:none;border-radius:8px;padding:8px 16px;color:white;font-size:13px;cursor:pointer;">
            <i class="fas fa-paper-plane"></i> Gửi
          </button>
        </div>
      </div>
    </div>
  `;

  document.body.appendChild(modal);
  document.body.style.overflow = "hidden";

  // Load comments
  profileSendToCSharp({ type: "GET_COMMENTS", data: { postId: postId } });

  // Đóng modal
  document.getElementById("closeProfileCommentModal").onclick = function () {
    modal.remove();
    document.body.style.overflow = "";
  };
  modal.onclick = function (e) {
    if (e.target === modal) {
      modal.remove();
      document.body.style.overflow = "";
    }
  };

  // Gửi bình luận
  var currentPostId = postId;
  function sendProfileComment() {
    var input = document.getElementById("profileCommentInput");
    var btn = document.getElementById("btnSendProfileComment");
    if (!input || !currentPostId) return;
    var content = input.value.trim();
    if (!content) { input.focus(); return; }
    if (btn) { btn.disabled = true; btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang gửi...'; }
    profileSendToCSharp({ type: "CREATE_COMMENT", data: { postId: currentPostId, content: content } });
  }

  document.getElementById("btnSendProfileComment").onclick = sendProfileComment;
  document.getElementById("profileCommentInput").onkeydown = function (e) {
    if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); sendProfileComment(); }
  };

  // Focus input
  setTimeout(function () {
    var input = document.getElementById("profileCommentInput");
    if (input) input.focus();
  }, 100);
}

// ============================================================
// SEARCH
// ============================================================

function initProfileSearch() {
  const input = document.getElementById("profileSearchInput");
  const results = document.getElementById("profileSearchResults");

  if (!input || !results) return;

  let searchTimer = null;

  input.addEventListener("input", function () {
    const keyword = input.value.trim();

    clearTimeout(searchTimer);

    if (!keyword) {
      results.classList.add("hidden");
      results.innerHTML = "";
      return;
    }

    searchTimer = setTimeout(function () {
      profileSendToCSharp({
        type: "SEARCH_USER",
        data: {
          keyword: keyword,
        },
      });
    }, 300);
  });

  document.addEventListener("click", function (event) {
    if (!event.target.closest(".profile-search-wrap")) {
      results.classList.add("hidden");
    }
  });
}

function renderProfileSearchResults(users) {
  const box = document.getElementById("profileSearchResults");

  if (!box) return;

  if (!users || users.length === 0) {
    box.innerHTML = `
      <div class="profile-search-item">
        <div>
          <strong>Không tìm thấy người dùng</strong>
          <span>Thử từ khóa khác</span>
        </div>
      </div>
    `;
    box.classList.remove("hidden");
    return;
  }

  let html = "";

  users.forEach(function (user) {
    html += `
      <div class="profile-search-item" data-user-id="${profileEscapeHTML(user.userId)}">
        <img
          src="${profileEscapeHTML(user.avatar || "https://i.pravatar.cc/150")}"
          alt="${profileEscapeHTML(user.userName || "User")}"
        />
        <div>
          <strong>${profileEscapeHTML(user.userName || "Ẩn danh")}</strong>
          <span>${profileEscapeHTML(user.phone || "")}</span>
        </div>
      </div>
    `;
  });

  box.innerHTML = html;
  box.classList.remove("hidden");

  document.querySelectorAll(".profile-search-item").forEach(function (item) {
    item.addEventListener("click", function () {
      const userId = item.dataset.userId;
      showProfileToast("Đã chọn người dùng: " + userId);

      profileSendToCSharp({
        type: "NAVIGATE_PROFILE",
        data: {
          userId: userId,
        },
      });

      box.classList.add("hidden");
    });
  });
}

function resetProfilePostModal() {
  const input = document.getElementById("profilePostContent");
  const visibility = document.getElementById("profilePostVisibility");
  const preview = document.getElementById("profileImagePreview");
  const publishBtn = document.getElementById("btnPublishProfilePost");

  if (input) input.value = "";
  if (visibility) visibility.value = "public";
  if (preview) preview.innerHTML = "";

  profileSelectedImagePath = null;

  if (publishBtn) {
    publishBtn.disabled = false;
    publishBtn.innerHTML = '<i class="fas fa-paper-plane"></i> Đăng bài';
  }
}

// ============================================================
// WEBVIEW2 RECEIVE MESSAGE
// ============================================================

let profileLoggedInUser = null;
let profileViewingUserId = null;

function initProfileWebViewMessages() {
  if (!(window.chrome && window.chrome.webview)) return;

  window.chrome.webview.addEventListener("message", function (event) {
    const msg = event.data;

    if (!msg || !msg.type) return;

    if (msg.type === "USER_UPDATED") {
      const data = msg.data || {};

      profileLoggedInUser = {
        userId: data.userId || "",
        userName: data.userName || "Người dùng",
        avatar: data.avatar || "",
        bio: data.bio || "",
      };

      setImage("topUserAvatar", profileLoggedInUser.avatar || "");

      updateProfileActionButtons();
      return;
    }

    if (msg.type === "PROFILE_DATA") {
      const data = msg.data || {};

      profileViewingUserId = data.userId != null ? String(data.userId) : "";

      renderProfileUser({
        userId: data.userId || "",
        userName: data.userName || "Người dùng",
        avatar: data.avatar || "",
        bio: data.bio || "",
      });

      if (Array.isArray(data.posts)) {
        renderProfilePosts(data.posts);
      }

      updateProfileStats(data.stats || {});
      updateProfileActionButtons();

      return;
    }

    if (msg.type === "PROFILE_POSTS_DATA") {
      renderProfilePosts(msg.data || []);
      return;
    }

    if (msg.type === "SEARCH_USER_RESULT") {
      renderProfileSearchResults(msg.data || []);
      return;
    }

    if (msg.type === "IMAGE_SELECTED") {
      handleProfileImageSelected(msg.data);
      return;
    }

    if (msg.type === "CREATE_POST_SUCCESS") {
      resetProfilePostModal();
      showProfileToast("Đăng bài thành công");
      closeProfilePostModal();

      if (profileViewingUserId) {
        profileSendToCSharp({
          type: "GET_USER_PROFILE",
          data: { userId: profileViewingUserId }
        });
      }

      return;
    }



    if (msg.type === "UPDATE_PROFILE_SUCCESS") {
      const data = msg.data || {};

      renderProfileUser({
        userId: data.userId || profileCurrentUser.userId,
        userName: data.userName || profileCurrentUser.userName,
        avatar: data.avatar || profileCurrentUser.avatar,
        bio: data.bio || profileCurrentUser.bio,
      });

      closeEditProfileModal();

      const saveBtn = document.getElementById("saveEditProfileBtn");
      if (saveBtn) {
        saveBtn.disabled = false;
        saveBtn.innerHTML = "Lưu thay đổi";
      }

      showProfileToast("Cập nhật hồ sơ thành công");
      return;
    }

    if (msg.type === "LIKE_UPDATED") {
      updateProfileLikeState(msg.data || {});
      return;
    }

    if (msg.type === "COMMENTS_DATA") {
      var data = msg.data || {};
      var list = document.getElementById("profileCommentList");
      if (!list) return;

      var comments = data.comments || [];
      if (comments.length === 0) {
        list.innerHTML = '<div style="color:#94a3b8;font-size:13px;text-align:center;padding:20px 0;">Chưa có bình luận nào.</div>';
        return;
      }

      var html = "";
      comments.forEach(function (c) {
        html += `
          <div style="display:flex;gap:10px;align-items:flex-start;">
            <img src="${profileEscapeHTML(c.avatar || 'https://i.pravatar.cc/100')}" style="width:32px;height:32px;border-radius:50%;object-fit:cover;flex-shrink:0;" onerror="this.src='https://i.pravatar.cc/100'" />
            <div style="flex:1;">
              <div style="background:rgba(255,255,255,0.07);border-radius:12px;padding:8px 12px;">
                <div style="font-weight:600;font-size:13px;color:#e2e8f0;margin-bottom:4px;">${profileEscapeHTML(c.userName || 'Ẩn danh')}</div>
                <div style="font-size:13px;color:#cbd5e1;">${profileEscapeHTML(c.content || '')}</div>
              </div>
              <div style="font-size:11px;color:#64748b;margin-top:4px;padding-left:4px;">${profileFormatTime(c.createdAt)}</div>
            </div>
          </div>
        `;
      });
      list.innerHTML = html;
      list.scrollTop = list.scrollHeight;
      return;
    }

    if (msg.type === "CREATE_COMMENT_SUCCESS") {
      var commentData = msg.data || {};
      var list2 = document.getElementById("profileCommentList");

      if (list2) {
        var empty = list2.querySelector('[style*="Chưa có bình luận"]');
        if (empty) empty.remove();

        var newHtml = `
          <div style="display:flex;gap:10px;align-items:flex-start;">
            <img src="${profileEscapeHTML(commentData.avatar || 'https://i.pravatar.cc/100')}" style="width:32px;height:32px;border-radius:50%;object-fit:cover;flex-shrink:0;" onerror="this.src='https://i.pravatar.cc/100'" />
            <div style="flex:1;">
              <div style="background:rgba(255,255,255,0.07);border-radius:12px;padding:8px 12px;">
                <div style="font-weight:600;font-size:13px;color:#e2e8f0;margin-bottom:4px;">${profileEscapeHTML(commentData.userName || 'Ẩn danh')}</div>
                <div style="font-size:13px;color:#cbd5e1;">${profileEscapeHTML(commentData.content || '')}</div>
              </div>
              <div style="font-size:11px;color:#64748b;margin-top:4px;padding-left:4px;">Vừa xong</div>
            </div>
          </div>
        `;
        list2.insertAdjacentHTML("beforeend", newHtml);
        list2.scrollTop = list2.scrollHeight;
      }

      // Cập nhật số bình luận trên post card
      var postId = commentData.postId;
      if (postId) {
        var postCard = document.querySelector('[data-post-id="' + postId + '"]');
        if (postCard) {
          var statsSpans = postCard.querySelectorAll('.profile-post-stats span');
          if (statsSpans[1]) {
            statsSpans[1].textContent = (commentData.commentCount || 0) + ' bình luận';
          }
        }
      }

      // Reset input và nút gửi
      var input = document.getElementById("profileCommentInput");
      var btn = document.getElementById("btnSendProfileComment");
      if (input) { input.value = ""; input.focus(); }
      if (btn) { btn.disabled = false; btn.innerHTML = '<i class="fas fa-paper-plane"></i> Gửi'; }
      return;
    }

    if (msg.type === "ERROR") {
      showProfileToast(msg.message || "Có lỗi xảy ra");

      // Re-enable nút save nếu đang bị disabled (sau UPDATE_PROFILE thất bại)
      var saveBtn = document.getElementById("saveEditProfileBtn");
      if (saveBtn && saveBtn.disabled) {
        saveBtn.disabled = false;
        saveBtn.innerHTML = "Lưu thay đổi";
      }

      // Re-enable nút publish nếu đang bị disabled
      var publishBtn = document.getElementById("btnPublishProfilePost");
      if (publishBtn && publishBtn.disabled) {
        publishBtn.disabled = false;
        publishBtn.innerHTML = '<i class="fas fa-paper-plane"></i> Đăng bài';
      }

      return;
    }
  });
}

function isMyProfile() {
  const loginId =
    profileLoggedInUser && profileLoggedInUser.userId != null
      ? String(profileLoggedInUser.userId)
      : "";

  const viewingId =
    profileViewingUserId != null
      ? String(profileViewingUserId)
      : "";

  return loginId !== "" && viewingId !== "" && loginId === viewingId;
}

function updateProfileActionButtons() {
  const editBtn = document.getElementById("btnEditProfile");
  const addStoryBtn = document.getElementById("btnAddStory");
  const canEdit = isMyProfile();

  if (editBtn) editBtn.style.display = canEdit ? "" : "none";
  if (addStoryBtn) addStoryBtn.style.display = canEdit ? "" : "none";
}

function handleProfileImageSelected(data) {
  profileSelectedImagePath = data && data.path ? data.path : null;

  const preview = document.getElementById("profileImagePreview");
  if (!preview) return;

  if (!profileSelectedImagePath) {
    preview.innerHTML = "";
    return;
  }

  preview.innerHTML = `
    <img src="file:///${profileEscapeHTML(profileSelectedImagePath)}" alt="Ảnh đã chọn" />
  `;
}

function updateProfileLikeState(data) {
  const postId = data.postId;
  const liked = data.liked;
  const likeCount = data.likeCount;

  if (!postId) return;

  const post = document.querySelector(`[data-post-id="${postId}"]`);
  if (!post) return;

  const likeBtn = post.querySelector(".profile-like-btn");
  const stats = post.querySelector(".profile-post-stats span:first-child");

  if (likeBtn) {
    if (liked) {
      likeBtn.classList.add("liked");
      const label = likeBtn.querySelector("span");
      const icon = likeBtn.querySelector("i");
      if (label) label.textContent = "Đã thích";
      if (icon) {
        icon.classList.remove("far");
        icon.classList.add("fas");
      }
    } else {
      likeBtn.classList.remove("liked");
      const label = likeBtn.querySelector("span");
      const icon = likeBtn.querySelector("i");
      if (label) label.textContent = "Thích";
      if (icon) {
        icon.classList.remove("fas");
        icon.classList.add("far");
      }
    }
  }

  if (stats) {
    stats.textContent = (likeCount || 0) + " lượt thích";
  }
}

function updateProfileStats(stats) {
  if (stats.posts != null) setText("statPosts", stats.posts);
  if (stats.friends != null) setText("statFriends", stats.friends);
  if (stats.likes != null) setText("statLikes", stats.likes);
  if (stats.comments != null) setText("statComments", stats.comments);
}

//Hàm mở Modal chỉnh sửa Profile
function openEditProfileModal() {
  const modal = document.getElementById("editProfileModal");
  const nameInput = document.getElementById("editProfileName");
  const bioInput = document.getElementById("editProfileBio");
  const avatarInput = document.getElementById("editProfileAvatar");
  const preview = document.getElementById("editProfileAvatarPreview");

  if (!modal) return;

  if (nameInput) nameInput.value = profileCurrentUser.userName || "";
  if (bioInput) bioInput.value = profileCurrentUser.bio || "";
  if (avatarInput) avatarInput.value = profileCurrentUser.avatar || "";
  if (preview) preview.src = profileCurrentUser.avatar || "https://i.pravatar.cc/150";

  modal.classList.remove("hidden");
}

function closeEditProfileModal() {
  const modal = document.getElementById("editProfileModal");
  if (modal) modal.classList.add("hidden");
}

function initEditProfileModal() {
  const editBtn = document.getElementById("btnEditProfile");
  const closeBtn = document.getElementById("editProfileCloseBtn");
  const cancelBtn = document.getElementById("cancelEditProfileBtn");
  const backdrop = document.getElementById("closeEditProfileModal");
  const saveBtn = document.getElementById("saveEditProfileBtn");
  const avatarInput = document.getElementById("editProfileAvatar");
  const preview = document.getElementById("editProfileAvatarPreview");

  if (editBtn) {
    editBtn.addEventListener("click", openEditProfileModal);
  }

  if (closeBtn) {
    closeBtn.addEventListener("click", closeEditProfileModal);
  }

  if (cancelBtn) {
    cancelBtn.addEventListener("click", closeEditProfileModal);
  }

  if (backdrop) {
    backdrop.addEventListener("click", closeEditProfileModal);
  }

  if (avatarInput && preview) {
    avatarInput.addEventListener("input", function () {
      preview.src = avatarInput.value || "https://i.pravatar.cc/150";
    });
  }

  if (saveBtn) {
    saveBtn.addEventListener("click", saveProfileChanges);
  }
}

function saveProfileChanges() {
  const nameInput = document.getElementById("editProfileName");
  const bioInput = document.getElementById("editProfileBio");
  const avatarInput = document.getElementById("editProfileAvatar");
  const saveBtn = document.getElementById("saveEditProfileBtn");

  const userName = nameInput ? nameInput.value.trim() : "";
  const bio = bioInput ? bioInput.value.trim() : "";
  const avatar = avatarInput ? avatarInput.value.trim() : "";

  if (!userName) {
    showProfileToast("Tên hiển thị không được để trống");
    return;
  }

  if (saveBtn) {
    saveBtn.disabled = true;
    saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';
  }

  profileSendToCSharp({
    type: "UPDATE_PROFILE",
    data: {
      userName: userName,
      bio: bio,
      avatar: avatar,
    },
  });
}

// ============================================================
// TOAST
// ============================================================

function showProfileToast(message, duration) {
  const toast = document.getElementById("profileToast");
  if (!toast) return;

  toast.textContent = message || "";

  toast.classList.add("show");

  setTimeout(function () {
    toast.classList.remove("show");
  }, duration || 2600);
}
