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
  }

  if (!date || isNaN(date.getTime())) return "Vừa xong";

  return date.toLocaleString("vi-VN");
}

let profileSelectedImagePath = null;
let profileCurrentUser = {
  userId: "",
  userName: "Khang Hoàng",
  avatar: "https://i.pravatar.cc/150?img=11",
  bio: "Mini Social App",
};

document.addEventListener("DOMContentLoaded", function () {
  initProfileSidebar();
  initProfileTabs();
  initProfilePostModal();
  initProfileLikeButtons();
  initProfileSearch();
  initProfileWebViewMessages();
  initProfileMenu();

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
    showProfileToast("Bạn đang ở trang cá nhân");
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
      <div id="profilePostsContainer" class="profile-posts-container"></div>
    `;

    renderProfilePosts(getDemoProfilePosts());
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
    openBtn.addEventListener("click", openProfilePostModal);
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
    publishBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang đăng...';
  }

  profileSendToCSharp({
    type: "CREATE_POST",
    data: {
      content: content,
      imagePath: profileSelectedImagePath,
      visibility: visibility ? visibility.value : "public",
    },
  });

  input.value = "";
  profileSelectedImagePath = null;

  const preview = document.getElementById("profileImagePreview");
  if (preview) preview.innerHTML = "";

  if (publishBtn) {
    publishBtn.disabled = false;
    publishBtn.innerHTML = '<i class="fas fa-paper-plane"></i> Đăng bài';
  }

  closeProfilePostModal();
  showProfileToast("Đăng bài thành công");
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
  const bio = profileCurrentUser.bio || "Mini Social App";

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

      ${
        post.mediaUrl
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
      const post = button.closest(".profile-post-card");
      const postId = post ? post.dataset.postId : null;

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
          data: {
            postId: postId,
          },
        });
      }
    };
  });
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
        type: "GET_USER_PROFILE",
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

function initProfileWebViewMessages() {
  if (!(window.chrome && window.chrome.webview)) return;

  window.chrome.webview.addEventListener("message", function (event) {
    const msg = event.data;

    if (!msg || !msg.type) return;

    if (msg.type === "USER_UPDATED") {
      const data = msg.data || {};

      renderProfileUser({
        userId: data.userId || "",
        userName: data.userName || "Người dùng",
        avatar: data.avatar || "",
        bio: data.bio || "Mini Social App",
      });

      return;
    }

    if (msg.type === "PROFILE_DATA") {
      const data = msg.data || {};

      renderProfileUser({
        userId: data.userId || "",
        userName: data.userName || "Người dùng",
        avatar: data.avatar || "",
        bio: data.bio || "Mini Social App",
      });

      if (Array.isArray(data.posts)) {
        renderProfilePosts(data.posts);
      }

      updateProfileStats(data.stats || {});
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
      profileSendToCSharp({ type: "GET_PROFILE_POSTS" });
      return;
    }

    if (msg.type === "LIKE_UPDATED") {
      updateProfileLikeState(msg.data || {});
      return;
    }

    if (msg.type === "ERROR") {
      showProfileToast(msg.message || "Có lỗi xảy ra");
      return;
    }
  });
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
