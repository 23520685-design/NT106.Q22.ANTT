// ============================================================
// WEBVIEW2 COMMUNICATION – đặt global để mọi hàm đều dùng được
// ============================================================
function sendToCSharp(message) {
  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.postMessage(message);
  } else {
    console.log("[DEV] WebView2 not available. Message:", message);
  }
}

// ============================================================
// FORMAT THỜI GIAN
// ============================================================
function formatTime(ts) {
  if (!ts) return "Vừa xong";

  let date = null;

  if (typeof ts === "number") {
    date = new Date(ts * 1000); // vì backend gửi seconds
  } else if (typeof ts === "string") {
    date = new Date(ts);
  }

  if (!date || isNaN(date.getTime())) return "Vừa xong";
  return date.toLocaleString("vi-VN");
}

// ============================================================
// SKELETON LOADER
// ============================================================
function showLoadingSkeleton() {
  const container = document.getElementById("postsContainer");
  if (!container) return;

  let html = "";
  for (let i = 0; i < 3; i++) {
    html += `
      <div class="post glass skeleton-post">
        <div class="post-header">
          <div class="post-author" style="gap:12px;display:flex;align-items:center">
            <div class="skeleton skeleton-avatar"></div>
            <div style="display:flex;flex-direction:column;gap:6px">
              <div class="skeleton skeleton-line" style="width:120px;height:13px"></div>
              <div class="skeleton skeleton-line" style="width:80px;height:11px"></div>
            </div>
          </div>
        </div>
        <div class="skeleton skeleton-line" style="width:100%;height:13px;margin-bottom:8px;margin-top:16px"></div>
        <div class="skeleton skeleton-line" style="width:75%;height:13px;margin-bottom:8px"></div>
        <div class="skeleton skeleton-line" style="width:50%;height:13px"></div>
      </div>`;
  }

  container.innerHTML = html;
}

// ============================================================
// RENDER FEED
// ============================================================
let _renderedPostIds = new Set();

function buildPostHTML(post) {
  const isLiked = post.isLiked === true;

  const visIcon =
    post.visibility === "private"
      ? `<i class="fas fa-lock" title="Riêng tư" style="margin-left:4px;font-size:10px;color:var(--text-muted)"></i>`
      : post.visibility === "followers"
        ? `<i class="fas fa-user-friends" title="Chỉ follower" style="margin-left:4px;font-size:10px;color:var(--text-muted)"></i>`
        : "";

  return `
    <article class="post glass" data-post-id="${post.postId}">
      <div class="resize-handle"></div>

      <div class="post-header">
        <div class="post-author">
          <img src="${post.avatar || "https://i.pravatar.cc/150?u=" + post.userId}"
               onerror="this.src='https://i.pravatar.cc/150'"
               alt="${post.userName || "User"}" />
          <div class="author-info">
            <h4>${post.userName || "Ẩn danh"}</h4>
            <span>${formatTime(post.createdAt)}${visIcon}</span>
          </div>
        </div>
        <button class="post-more glass-btn">
          <i class="fas fa-ellipsis-h"></i>
        </button>
      </div>

      <div class="post-content">
        <p>${post.content || ""}</p>
        ${
          post.mediaUrl
            ? `<div class="post-image"><img src="${post.mediaUrl}" alt="Ảnh bài viết" loading="lazy" /></div>`
            : ""
        }
      </div>

      <div class="post-stats">
        <div class="reactions">
          <span class="reaction-icons">❤️👍</span>
          <span>${post.likeCount || 0} lượt thích</span>
        </div>
        <div class="comments-shares">
          <span>${post.commentCount || 0} bình luận</span>
        </div>
      </div>

      <div class="post-buttons">
        <button class="post-btn like-btn ${isLiked ? "liked" : ""}">
          <i class="${isLiked ? "fas" : "far"} fa-thumbs-up"></i>
          <span>Thích</span>
        </button>
        <button class="post-btn comment-btn">
          <i class="far fa-comment-alt"></i>
          <span>Bình luận</span>
        </button>
        <button class="post-btn share-btn">
          <i class="far fa-share-square"></i>
          <span>Chia sẻ</span>
        </button>
      </div>
    </article>`;
}

// ============================================================
// DIFF RENDER FEED
// ============================================================
function renderFeed(posts) {
  const container = document.getElementById("postsContainer");
  if (!container) return;

  if (!posts || posts.length === 0) {
    container.innerHTML = `
      <div class="empty-feed">
        <i class="fas fa-newspaper"></i>
        <p>Chưa có bài viết nào. Hãy là người đầu tiên đăng bài!</p>
      </div>`;
    _renderedPostIds.clear();
    return;
  }

  const isFirstRender =
    container.querySelectorAll(".post[data-post-id]").length === 0;

  if (isFirstRender) {
    let allHTML = "";
    posts.forEach((post) => {
      allHTML += buildPostHTML(post);
    });

    container.innerHTML = allHTML;
    _renderedPostIds = new Set(posts.map((p) => p.postId));
    bindPostEvents();
    bindModalEvents();
    return;
  }

  let hasNewPosts = false;
  const fragment = document.createDocumentFragment();

  posts.forEach((post) => {
    const postId = post.postId;

    if (_renderedPostIds.has(postId)) {
      const existingPost = container.querySelector(
        `[data-post-id="${postId}"]`,
      );

      if (existingPost) {
        const countEl = existingPost.querySelector(
          ".reactions span:last-child",
        );
        if (countEl) {
          countEl.textContent = (post.likeCount || 0) + " lượt thích";
        }

        const commentEl = existingPost.querySelector(".comments-shares span");
        if (commentEl) {
          commentEl.textContent = (post.commentCount || 0) + " bình luận";
        }

        const likeBtn = existingPost.querySelector(".like-btn");
        const icon = likeBtn ? likeBtn.querySelector("i") : null;

        if (likeBtn && icon) {
          if (post.isLiked === true) {
            likeBtn.classList.add("liked");
            icon.classList.remove("far");
            icon.classList.add("fas");
          } else {
            likeBtn.classList.remove("liked");
            icon.classList.remove("fas");
            icon.classList.add("far");
          }
        }
      }
    } else {
      const temp = document.createElement("div");
      temp.innerHTML = buildPostHTML(post);
      const articleEl = temp.firstElementChild;

      if (fragment.prepend) {
        fragment.prepend(articleEl);
      } else {
        fragment.appendChild(articleEl);
      }

      _renderedPostIds.add(postId);
      hasNewPosts = true;
    }
  });

  if (hasNewPosts) {
    container.insertBefore(fragment, container.firstChild);
    bindPostEvents();
    bindModalEvents();
  }
}

// ============================================================
// BIND EVENTS SAU KHI RENDER
// ============================================================
function bindPostEvents() {
  document.querySelectorAll(".post-btn").forEach((btn) => {
    btn.onclick = function (e) {
      e.stopPropagation();

      const text = this.querySelector("span");
      if (!text || text.textContent !== "Thích") return;

      const post = this.closest(".post");
      const postId = post && post.dataset ? post.dataset.postId : null;
      if (!postId) return;

      if (this.dataset.loading === "true") return;
      this.dataset.loading = "true";

      const icon = this.querySelector("i");

      if (this.classList.contains("liked")) {
        this.classList.remove("liked");
        if (icon) {
          icon.classList.remove("fas");
          icon.classList.add("far");
        }
      } else {
        this.classList.add("liked");
        if (icon) {
          icon.classList.remove("far");
          icon.classList.add("fas");
        }
      }

      sendToCSharp({ type: "TOGGLE_LIKE", data: { postId: postId } });

      setTimeout(() => {
        this.dataset.loading = "false";
      }, 100);
    };
  });
}

function bindModalEvents() {
  const modal = document.getElementById("postModal");
  const modalContent = document.getElementById("modalContent");

  document.querySelectorAll(".post").forEach((post) => {
    const postContent = post.querySelector(".post-content");
    const postImage = post.querySelector(".post-image");

    if (postContent) {
      postContent.addEventListener("click", (e) => {
        if (!e.target.closest(".post-btn") && !e.target.closest(".post-more")) {
          openPostModal(post, modal, modalContent);
        }
      });
    }

    if (postImage) {
      postImage.addEventListener("click", (e) => {
        e.stopPropagation();
        openPostModal(post, modal, modalContent);
      });
    }

    post
      .querySelectorAll(".post-btn, .post-more, .resize-handle")
      .forEach((el) => {
        el.addEventListener("click", (e) => e.stopPropagation());
      });
  });
}

// ============================================================
// MODAL
// ============================================================
function openPostModal(post, modal, modalContent) {
  if (!modal || !modalContent) return;

  const clone = post.cloneNode(true);
  const resizeHandle = clone.querySelector(".resize-handle");
  if (resizeHandle) resizeHandle.remove();

  const freshClone = clone.cloneNode(true);

  const closeBtn = document.createElement("button");
  closeBtn.className = "modal-close";
  closeBtn.innerHTML = '<i class="fas fa-times"></i>';
  closeBtn.onclick = function () {
    closePostModal(modal);
  };

  modalContent.innerHTML = "";
  modalContent.appendChild(closeBtn);
  modalContent.appendChild(freshClone);

  modal.classList.remove("hidden");
  document.body.style.overflow = "hidden";
}

function closePostModal(modal) {
  if (modal) {
    modal.classList.add("hidden");
    document.body.style.overflow = "";
  }
}

// ============================================================
// TOAST NOTIFICATION
// ============================================================
function showToast(message, duration) {
  if (duration == null) duration = 3000;

  let toast = document.getElementById("toastNotification");
  if (!toast) {
    toast = document.createElement("div");
    toast.id = "toastNotification";
    document.body.appendChild(toast);
  }

  toast.textContent = message;
  toast.className = "toast-notification show";

  setTimeout(() => {
    toast.className = "toast-notification";
  }, duration);
}

// ============================================================
// LOAD FEED – HÀM GLOBAL
// ============================================================
function loadFeed() {
  sendToCSharp({ type: "GET_FEED" });
}

// ============================================================
// MAIN – DOMContentLoaded
// ============================================================
document.addEventListener("DOMContentLoaded", function () {
  const sidebar = document.getElementById("sidebarLeft");
  const sidebarRight = document.getElementById("sidebarRight");
  const createPostSection = document.getElementById("createPostSection");
  const btnPublish = document.getElementById("btnPublish");

  let selectedImagePath = null;

  // ---- Sidebar toggles ----
  const toggleSidebar = document.getElementById("toggleSidebar");
  if (toggleSidebar) {
    toggleSidebar.addEventListener("click", () => {
      if (sidebar) sidebar.classList.toggle("collapsed");
      setTimeout(updatePostsForAvailableSpace, 400);
    });
  }

  const toggleSidebarRight = document.getElementById("toggleSidebarRight");
  if (toggleSidebarRight) {
    toggleSidebarRight.addEventListener("click", () => {
      if (sidebarRight) sidebarRight.classList.toggle("collapsed");
      setTimeout(updatePostsForAvailableSpace, 400);
    });
  }

  // ---- Menu navigation ----
  const menuLinks = document.querySelectorAll(".menu-link");
  const miniIcons = document.querySelectorAll(".mini-icons i");

  function setActivePage() {
    menuLinks.forEach((l) => {
      if (l.parentElement) l.parentElement.classList.remove("active");
    });
    miniIcons.forEach((i) => i.classList.remove("active"));
  }

  function handlePageChange(page) {
    switch (page) {
      case "create-post":
        showCreatePost();
        break;
      case "home":
        showHome();
        break;
      case "logout":
        handleLogout();
        break;
      default:
        if (createPostSection) createPostSection.style.display = "none";
        break;
    }
  }

  function showCreatePost() {
    if (createPostSection) {
      createPostSection.style.display = "block";
      createPostSection.scrollIntoView({ behavior: "smooth", block: "start" });
    }
  }

  function showHome() {
    if (createPostSection) {
      createPostSection.style.display = "none";
    }
  }

  function handleLogout() {
    if (confirm("Bạn có chắc muốn đăng xuất?")) {
      sendToCSharp({ type: "LOGOUT" });
    }
  }

  menuLinks.forEach((link) => {
    link.addEventListener("click", (e) => {
      e.preventDefault();
      const page = link.getAttribute("data-page");
      setActivePage();
      if (link.parentElement) link.parentElement.classList.add("active");
      handlePageChange(page);
    });
  });

  miniIcons.forEach((icon) => {
    icon.addEventListener("click", () => {
      const page = icon.getAttribute("data-page");
      setActivePage();
      icon.classList.add("active");

      const linkedMenu = document.querySelector(
        `.menu-link[data-page="${page}"]`,
      );
      if (linkedMenu && linkedMenu.parentElement) {
        linkedMenu.parentElement.classList.add("active");
      }

      handlePageChange(page);
    });
  });

  // ---- Chọn ảnh ----
  const btnChooseImage = document.getElementById("btnChooseImage");
  if (btnChooseImage) {
    btnChooseImage.addEventListener("click", () => {
      sendToCSharp({ type: "CHOOSE_IMAGE" });
    });
  }

  // ---- Đóng form đăng bài ----
  const closeCreatePost = document.getElementById("closeCreatePost");
  if (closeCreatePost) {
    closeCreatePost.addEventListener("click", () => {
      if (createPostSection) createPostSection.style.display = "none";
    });
  }

  // ---- Đăng bài ----
  if (btnPublish) {
    btnPublish.addEventListener("click", () => {
      const contentEl = document.getElementById("postContent");
      const visibilityEl = document.getElementById("postVisibility");
      const content = contentEl ? contentEl.value : "";

      if (!content.trim() && !selectedImagePath) {
        if (contentEl) {
          contentEl.focus();
          contentEl.style.borderColor = "#ef4444";
          setTimeout(() => {
            contentEl.style.borderColor = "";
          }, 1000);
        }
        return;
      }

      btnPublish.innerHTML =
        '<i class="fas fa-spinner fa-spin"></i> Đang đăng...';
      btnPublish.disabled = true;

      sendToCSharp({
        type: "CREATE_POST",
        data: {
          content: content,
          imagePath: selectedImagePath,
          visibility: visibilityEl ? visibilityEl.value : "public",
        },
      });
    });
  }

  // ---- Modal – click ngoài / ESC đóng ----
  const modal = document.getElementById("postModal");
  if (modal) {
    modal.addEventListener("click", (e) => {
      if (e.target === modal) closePostModal(modal);
    });
  }

  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape" && modal && !modal.classList.contains("hidden")) {
      closePostModal(modal);
    }
  });

  // ---- Resize posts ----
  function updatePostsForAvailableSpace() {
    const slw = sidebar && sidebar.classList.contains("collapsed") ? 70 : 260;
    const srw =
      sidebarRight && sidebarRight.classList.contains("collapsed") ? 70 : 320;
    const avail = window.innerWidth - slw - srw - 200;

    document.querySelectorAll(".post").forEach((post) => {
      const leftOff = sidebar && sidebar.classList.contains("collapsed");
      const rightOff =
        sidebarRight && sidebarRight.classList.contains("collapsed");

      if (leftOff && rightOff) {
        post.style.maxWidth = Math.min(700, avail) + "px";
        post.style.width = "60%";
      } else if (leftOff || rightOff) {
        post.style.maxWidth = Math.min(480, avail) + "px";
        post.style.width = "";
      } else {
        post.style.maxWidth = "480px";
        post.style.width = "";
      }
    });
  }

  let resizeTimeout = null;
  window.addEventListener("resize", () => {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(updatePostsForAvailableSpace, 100);
  });

  // ---- Friend buttons ----
  document.querySelectorAll(".message-btn").forEach((btn) => {
    btn.addEventListener("click", (e) => {
      e.stopPropagation();
      const friendItem = btn.closest(".friend-item");
      const nameEl = friendItem
        ? friendItem.querySelector(".friend-name")
        : null;
      const name = nameEl ? nameEl.textContent : null;
      if (name) console.log("Mở chat với:", name);
    });
  });

  document.querySelectorAll(".add-btn").forEach((btn) => {
    btn.addEventListener("click", (e) => {
      e.stopPropagation();
      btn.innerHTML = '<i class="fas fa-check"></i>';
      btn.style.background = "#10b981";
      btn.style.color = "white";
    });
  });

  // ---- WEBVIEW2 – nhận message từ C# ----
  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener("message", function (event) {
      const msg = event.data;
      console.log("[C# → JS]:", msg);

      // FEED DATA
      if (msg.type === "FEED_DATA") {
        renderFeed(msg.data);
        updatePostsForAvailableSpace();
        return;
      }

      // USER UPDATED
      if (msg.type === "USER_UPDATED") {
        const data = msg.data || {};
        const userName = data.userName || "User";
        const avatar = data.avatar || "";

        window.currentUser = {
          userName: userName,
          avatar: avatar,
        };

        const headerAvatar = document.querySelector(".user-avatar img");
        if (headerAvatar && avatar) headerAvatar.src = avatar;

        const postAvatar = document.querySelector(".post-avatar-large");
        if (postAvatar && avatar) postAvatar.src = avatar;

        const textarea = document.getElementById("postContent");
        if (textarea) {
          textarea.placeholder = `Bạn đang nghĩ gì, ${userName}?`;
        }

        const sbName = document.getElementById("sidebarUserName");
        const sbAvatar = document.getElementById("sidebarUserAvatar");
        if (sbName) sbName.textContent = userName;
        if (sbAvatar && avatar) sbAvatar.src = avatar;

        showLoadingSkeleton();
        loadFeed();
        return;
      }

      // LIKE UPDATED
      if (msg.type === "LIKE_UPDATED") {
        const data = msg.data || {};
        const postId = data.postId;
        const liked = data.liked;
        const likeCount = data.likeCount;

        const post = document.querySelector(`[data-post-id="${postId}"]`);
        if (!post) return;

        const likeBtn = post.querySelector(".like-btn");
        const icon = likeBtn ? likeBtn.querySelector("i") : null;
        const countEl = post.querySelector(".reactions span:last-child");

        if (likeBtn && icon) {
          if (liked) {
            likeBtn.classList.add("liked");
            icon.classList.remove("far");
            icon.classList.add("fas");
          } else {
            likeBtn.classList.remove("liked");
            icon.classList.remove("fas");
            icon.classList.add("far");
          }
        }

        if (countEl) {
          countEl.textContent = (likeCount || 0) + " lượt thích";
        }

        return;
      }

      // CREATE POST SUCCESS
      if (msg.type === "CREATE_POST_SUCCESS") {
        const contentEl = document.getElementById("postContent");
        const visEl = document.getElementById("postVisibility");
        const previewEl = document.getElementById("imagePreview");

        if (contentEl) contentEl.value = "";
        if (visEl) visEl.value = "public";

        selectedImagePath = null;

        if (previewEl) previewEl.innerHTML = "";

        if (btnPublish) {
          btnPublish.innerHTML = '<i class="fas fa-paper-plane"></i> Đăng bài';
          btnPublish.disabled = false;
        }

        if (createPostSection) {
          createPostSection.style.display = "none";
        }

        setActivePage();
        const homeMenu = document.querySelector('.menu-link[data-page="home"]');
        if (homeMenu && homeMenu.parentElement) {
          homeMenu.parentElement.classList.add("active");
        }

        showToast("Đăng bài thành công!");
        showLoadingSkeleton();
        loadFeed();
        return;
      }

      // IMAGE SELECTED
      if (msg.type === "IMAGE_SELECTED") {
        const previewEl = document.getElementById("imagePreview");
        selectedImagePath = msg.data ? msg.data.path : null;

        if (!previewEl) return;

        if (selectedImagePath) {
          previewEl.innerHTML = `
            <div style="position:relative;width:100%;max-width:700px;margin-top:10px;">
              <img src="file:///${selectedImagePath}"
                   style="width:100%;max-height:400px;object-fit:cover;border-radius:10px;display:block;" />
              <button id="removeImage" style="
                position:absolute;top:8px;right:8px;background:rgba(0,0,0,0.6);
                color:white;border:none;border-radius:50%;width:30px;height:30px;
                cursor:pointer;font-size:16px;line-height:1;">×</button>
            </div>`;

          const removeBtn = document.getElementById("removeImage");
          if (removeBtn) {
            removeBtn.onclick = function () {
              selectedImagePath = null;
              previewEl.innerHTML = "";
            };
          }
        } else {
          previewEl.innerHTML = "";
        }

        return;
      }

      // ERROR
      if (msg.type === "ERROR") {
        if (btnPublish) {
          btnPublish.innerHTML = '<i class="fas fa-paper-plane"></i> Đăng bài';
          btnPublish.disabled = false;
        }

        showToast(msg.message || "Có lỗi xảy ra");
        return;
      }

      // LOGOUT SUCCESS
      if (msg.type === "LOGOUT_SUCCESS") {
        console.log("Đăng xuất thành công");
      }
    });
  }

  // ---- GLOBAL API ----
  window.socialApp = {
    refreshFeed: function () {
      showLoadingSkeleton();
      loadFeed();
    },
    toggleLeftSidebar: function () {
      if (sidebar) sidebar.classList.toggle("collapsed");
      setTimeout(updatePostsForAvailableSpace, 400);
    },
    toggleRightSidebar: function () {
      if (sidebarRight) sidebarRight.classList.toggle("collapsed");
      setTimeout(updatePostsForAvailableSpace, 400);
    },
  };

  // ---- INIT ----
  const homeMenu = document.querySelector('.menu-link[data-page="home"]');
  if (homeMenu && homeMenu.parentElement) {
    homeMenu.parentElement.classList.add("active");
  }

  showLoadingSkeleton();
  console.log("Social Mini App initialized!");
});
