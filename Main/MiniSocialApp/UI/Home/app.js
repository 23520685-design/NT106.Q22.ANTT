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

function escapeHTML(value) {
  return String(value || "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
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
// Modal hiện tại đang mở post nào (dùng để cập nhật comment realtime nếu có)
let _currentModalPostId = null;
let _currentModalPostElement = null;
let _commentsCache = {};

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
          <img src="${escapeHTML(post.avatar || "https://i.pravatar.cc/150?u=" + post.userId)}"
               onerror="this.src='https://i.pravatar.cc/150'"
               alt="${escapeHTML(post.userName || "User")}" />
          <div class="author-info">
            <h4>${escapeHTML(post.userName || "Ẩn danh")}</h4>
            <span>${formatTime(post.createdAt)}${visIcon}</span>
          </div>
        </div>
        <button class="post-more glass-btn">
          <i class="fas fa-ellipsis-h"></i>
        </button>
      </div>

      <div class="post-content">
        <p>${escapeHTML(post.content || "")}</p>
        ${
          post.mediaUrl
            ? `<div class="post-image"><img src="${escapeHTML(post.mediaUrl)}" alt="Ảnh bài viết" loading="lazy" /></div>`
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

// Hàm này sẽ được gọi sau khi mở modal để bind các nút like/comment/share trong modal
function bindModalPostActions(modalContent) {
  if (!modalContent) return;

  const modalPost = modalContent.querySelector(".post");
  if (!modalPost) return;

  const postId = modalPost.dataset ? modalPost.dataset.postId : null;
  if (!postId) return;

  const likeBtn = modalPost.querySelector(".like-btn");
  const commentBtn = modalPost.querySelector(".comment-btn");
  const shareBtn = modalPost.querySelector(".share-btn");

  // LIKE trong modal
  if (likeBtn) {
    likeBtn.onclick = function (e) {
      e.stopPropagation();

      if (this.dataset.loading === "true") return;
      this.dataset.loading = "true";

      const icon = this.querySelector("i");

      // Optimistic UI: đổi giao diện trước cho mượt
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

      sendToCSharp({
        type: "TOGGLE_LIKE",
        data: { postId: postId },
      });

      setTimeout(() => {
        this.dataset.loading = "false";
      }, 100);
    };
  }

  // COMMENT trong modal -> focus ô nhập bình luận
  if (commentBtn) {
    commentBtn.onclick = function (e) {
      e.stopPropagation();
      const input = document.getElementById("commentInput");
      if (input) input.focus();
    };
  }

  // SHARE tạm thời chưa có chức năng
  if (shareBtn) {
    shareBtn.onclick = function (e) {
      e.stopPropagation();
      showToast("Chức năng chia sẻ chưa được hỗ trợ");
    };
  }
}

// Hàm này bind các nút like/comment/share ở mỗi post trong feed (không phải trong modal)
function bindPostEvents() {
  document.querySelectorAll(".post-btn").forEach((btn) => {
    btn.onclick = function (e) {
      e.stopPropagation();

      const post = this.closest(".post");
      const postId = post && post.dataset ? post.dataset.postId : null;
      if (!postId) return;

      // LIKE
      if (this.classList.contains("like-btn")) {
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

        return;
      }

      // COMMENT
      if (this.classList.contains("comment-btn")) {
        const modal = document.getElementById("postModal");
        const modalContent = document.getElementById("modalContent");
        openPostModal(post, modal, modalContent, true);
        return;
      }
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
function openPostModal(post, modal, modalContent, autoFocusComment) {
  if (!modal || !modalContent || !post) return;

  const postId = post.dataset ? post.dataset.postId : null;
  _currentModalPostId = postId;
  _currentModalPostElement = post;

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
  bindModalPostActions(modalContent);

  if (postId) {
    const wrapper = document.createElement("div");
    wrapper.innerHTML = buildCommentsSectionHTML(postId);
    modalContent.appendChild(wrapper.firstElementChild);
  }

  modal.classList.remove("hidden");
  document.body.style.overflow = "hidden";

  bindCommentEvents();

  if (postId) {
    requestComments(postId);
  }

  if (autoFocusComment) {
    setTimeout(() => {
      const input = document.getElementById("commentInput");
      if (input) input.focus();
    }, 100);
  }
}

function closePostModal(modal) {
  if (modal) {
    modal.classList.add("hidden");
    document.body.style.overflow = "";
  }

  _currentModalPostId = null;
  _currentModalPostElement = null;
}

// ============================================================
// COMMENT FUNCTIONS – global scope (gọi được từ openPostModal)
// ============================================================
function buildCommentHTML(comment) {
  return `
  <div class="comment-item" data-comment-id="${comment.commentId || ""}">
    <img
      class="comment-avatar"
      src="${escapeHTML(comment.avatar || "https://i.pravatar.cc/100?u=" + (comment.userId || "user"))}"
      alt="${escapeHTML(comment.userName || "User")}"
      onerror="this.src='https://i.pravatar.cc/100'"
    />
    <div class="comment-body">
      <div class="comment-bubble">
        <div class="comment-author">${escapeHTML(comment.userName || "Ẩn danh")}</div>
        <div class="comment-text">${escapeHTML(comment.content || "")}</div>
      </div>
      <div class="comment-meta">${formatTime(comment.createdAt)}</div>
    </div>
  </div>
  `;
}

function buildCommentsSectionHTML(postId) {
  const avatarSrc =
    window.currentUser && window.currentUser.avatar
      ? window.currentUser.avatar
      : "https://i.pravatar.cc/100?u=current";
  return `
  <section class="comments-section" data-post-id="${postId}">
    <div class="comments-list" id="commentsList">
      <div class="comments-loading">Đang tải bình luận...</div>
    </div>
    <div class="comment-input-wrap">
      <img
        class="comment-input-avatar"
        src="${avatarSrc}"
        alt="Bạn"
        onerror="this.src='https://i.pravatar.cc/100'"
      />
      <div class="comment-input-box">
        <textarea
          id="commentInput"
          class="comment-input"
          placeholder="Viết bình luận..."
          rows="2"
        ></textarea>
        <div class="comment-actions">
          <button id="btnSendComment" class="btn-send-comment">
            <i class="fas fa-paper-plane"></i> Gửi
          </button>
        </div>
      </div>
    </div>
  </section>
  `;
}

function renderComments(postId, comments) {
  const list = document.getElementById("commentsList");
  if (!list) return;

  if (!comments || comments.length === 0) {
    list.innerHTML = `<div class="comments-empty">Chưa có bình luận nào.</div>`;
    return;
  }

  let html = "";
  comments.forEach(function (comment) {
    html += buildCommentHTML(comment);
  });

  list.innerHTML = html;
  list.scrollTop = list.scrollHeight;
}

function appendCommentToModal(comment) {
  const list = document.getElementById("commentsList");
  if (!list) return;

  const empty = list.querySelector(".comments-empty");
  const loading = list.querySelector(".comments-loading");
  if (empty) empty.remove();
  if (loading) loading.remove();

  list.insertAdjacentHTML("beforeend", buildCommentHTML(comment));
  list.scrollTop = list.scrollHeight;
}

function updatePostCommentCount(postId, commentCount) {
  const posts = document.querySelectorAll(`[data-post-id="${postId}"]`);
  posts.forEach(function (post) {
    const commentEl = post.querySelector(".comments-shares span");
    if (commentEl) {
      commentEl.textContent = (commentCount || 0) + " bình luận";
    }
  });
}

function requestComments(postId) {
  if (!postId) return;
  sendToCSharp({
    type: "GET_COMMENTS",
    data: { postId: postId },
  });
}

function submitComment() {
  const input = document.getElementById("commentInput");
  if (!input || !_currentModalPostId) return;

  const content = input.value.trim();
  if (!content) {
    input.focus();
    return;
  }

  const btn = document.getElementById("btnSendComment");
  if (btn) {
    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang gửi...';
  }

  sendToCSharp({
    type: "CREATE_COMMENT",
    data: {
      postId: _currentModalPostId,
      content: content,
    },
  });
}

// ============================================================
// SEARCH USER
// ============================================================

function searchUser(keyword) {
  sendToCSharp({
    type: "SEARCH_USER",
    data: {
      keyword: keyword,
    },
  });
}

function renderSearchResults(users) {
  const box = document.getElementById("searchResults");
  if (!box) return;

  if (!users || users.length === 0) {
    box.innerHTML = `<div class="search-empty">Không tìm thấy người dùng</div>`;
    box.classList.remove("hidden");
    return;
  }

  let html = "";

  users.forEach((user) => {
    html += `
      <div class="search-result-item" data-user-id="${user.userId}">
        <img 
          src="${escapeHTML(user.avatar || "https://i.pravatar.cc/150?u=" + user.userId)}" 
          alt="${escapeHTML(user.userName || "User")}"
          onerror="this.src='https://i.pravatar.cc/150'"
        />
        <div class="search-result-info">
          <div class="search-result-name">${escapeHTML(user.userName || "Ẩn danh")}</div>
          <div class="search-result-sub">${escapeHTML(user.phone || "")}</div>
        </div>
      </div>
    `;
  });

  box.innerHTML = html;
  box.classList.remove("hidden");

  document.querySelectorAll(".search-result-item").forEach((item) => {
    item.onclick = function () {
      const userId = this.dataset.userId;
      showToast("Đã chọn user: " + userId);

      // Sau này chỗ này sẽ mở profile
      // sendToCSharp({ type: "GET_USER_PROFILE", data: { userId: userId } });
    };
  });
}

// ============================================================
// COMMENT EVENTS – bind sau khi mở modal
// ============================================================

function bindCommentEvents() {
  const btn = document.getElementById("btnSendComment");
  const input = document.getElementById("commentInput");

  if (btn) {
    btn.onclick = function () {
      submitComment();
    };
  }

  if (input) {
    input.onkeydown = function (e) {
      if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault();
        submitComment();
      }
    };
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

  // ---- Search user ----
  // Tìm kiếm người dùng khi nhập vào ô search, có debounce 300ms để tránh gửi quá nhiều request
  const searchInput = document.getElementById("searchInput");
  const searchResults = document.getElementById("searchResults");
  let searchTimer = null;

  if (searchInput) {
    searchInput.addEventListener("input", function () {
      const keyword = this.value.trim();

      clearTimeout(searchTimer);

      if (!keyword) {
        if (searchResults) {
          searchResults.classList.add("hidden");
          searchResults.innerHTML = "";
        }
        return;
      }

      searchTimer = setTimeout(() => {
        searchUser(keyword);
      }, 300);
    });

    searchInput.addEventListener("keydown", function (e) {
      if (e.key === "Enter") {
        e.preventDefault();

        const keyword = this.value.trim();
        if (keyword) {
          searchUser(keyword);
        }
      }
    });
  }

  document.addEventListener("click", function (e) {
    if (searchResults && !e.target.closest(".search-box")) {
      searchResults.classList.add("hidden");
    }
  });

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

        const posts = document.querySelectorAll(`[data-post-id="${postId}"]`);
        if (!posts || posts.length === 0) return;

        posts.forEach((post) => {
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
        });

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

      // COMMENTS DATA
      if (msg.type === "COMMENTS_DATA") {
        const data = msg.data || {};
        const postId = data.postId;
        const comments = data.comments || [];

        _commentsCache[postId] = comments;
        renderComments(postId, comments);
        return;
      }

      // CREATE COMMENT SUCCESS
      if (msg.type === "CREATE_COMMENT_SUCCESS") {
        const data = msg.data || {};
        const postId = data.postId;

        if (!_commentsCache[postId]) {
          _commentsCache[postId] = [];
        }

        _commentsCache[postId].push(data);
        appendCommentToModal(data);
        updatePostCommentCount(postId, data.commentCount || 0);

        const input = document.getElementById("commentInput");
        if (input) {
          input.value = "";
          input.focus();
        }

        const btn = document.getElementById("btnSendComment");
        if (btn) {
          btn.disabled = false;
          btn.innerHTML = '<i class="fas fa-paper-plane"></i> Gửi';
        }

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
                   style="width:100%;height:auto;object-fit:contain;border-radius:10px;display:block;" />
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

      // SEARCH USER RESULT
      if (msg.type === "SEARCH_USER_RESULT") {
        renderSearchResults(msg.data || []);
        return;
      }

      // ERROR
      if (msg.type === "ERROR") {
        if (btnPublish) {
          btnPublish.innerHTML = '<i class="fas fa-paper-plane"></i> Đăng bài';
          btnPublish.disabled = false;
        }

        // Reset nút gửi comment nếu gửi comment bị lỗi
        const sendBtn = document.getElementById("btnSendComment");
        if (sendBtn) {
          sendBtn.disabled = false;
          sendBtn.innerHTML = '<i class="fas fa-paper-plane"></i> Gửi';
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
