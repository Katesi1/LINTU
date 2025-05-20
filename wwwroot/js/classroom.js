// Classroom JS - Quản lý hiển thị Lessons và Lectures

/**
 * Tải danh sách bài học cho lớp học
 * @param {number} classRoomId - ID của lớp học
 */
function loadLessons(classRoomId) {
  if (!classRoomId) {
    console.error("Không tìm thấy ID lớp học");
    return;
  }

  console.log("Đang tải bài học cho lớp ID:", classRoomId);

  const lessonsContainer = document.getElementById("lessonsAccordion");
  if (!lessonsContainer) {
    console.error("Không tìm thấy container accordion");
    return;
  }

  // Hiển thị loading
  document.getElementById("lessonsLoading").style.display = "block";
  document.getElementById("noLessonsMessage").classList.add("d-none");

  // Gọi API để lấy danh sách bài học
  $.ajax({
    url: "/Lessons/GetLessons",
    type: "GET",
    data: { classRoomId: classRoomId },
    dataType: "json",
    cache: false,
    success: function (response) {
      console.log("Kết quả API GetLessons:", response);

      // Ẩn thông báo loading
      document.getElementById("lessonsLoading").style.display = "none";

      // Xử lý lỗi nếu có
      if (!response.success) {
        showToast(response.message || "Không thể tải bài học", "error");
        document.getElementById("noLessonsMessage").classList.remove("d-none");
        return;
      }

      // Lấy dữ liệu lessons
      const lessons = response.data || [];

      // Hiển thị thông báo nếu không có bài học
      if (lessons.length === 0) {
        document.getElementById("noLessonsMessage").classList.remove("d-none");
        return;
      }

      // Tạo HTML cho mỗi lesson
      let lessonsHtml = "";
      lessons.forEach((lesson, index) => {
        const headingId = `heading-${lesson.id}`;
        const collapseId = `collapse-${lesson.id}`;
        const lectureCount = lesson.lectureCount || 0;
        const lectureText =
          lectureCount === 1 ? "1 bài giảng" : `${lectureCount} bài giảng`;

        lessonsHtml += `
                    <div class="card border-0 shadow-sm mb-3" data-lesson-id="${
                      lesson.id
                    }">
                        <div class="card-header bg-white p-0" id="${headingId}">
                            <div class="d-flex justify-content-between align-items-center p-3" 
                                data-toggle="collapse" data-target="#${collapseId}" 
                                aria-expanded="${
                                  index === 0 ? "true" : "false"
                                }" aria-controls="${collapseId}" 
                                style="cursor: pointer;">
                                <div class="d-flex align-items-center">
                                    <div class="me-3 text-primary">
                                        <i class="fas fa-layer-group fa-lg"></i>
                                    </div>
                                    <div>
                                        <h5 class="mb-0 fw-bold">${
                                          lesson.title || "Bài học chưa đặt tên"
                                        }</h5>
                                        <small class="text-muted">${lectureText} • ${
          lesson.totalDuration || 0
        } phút</small>
                                    </div>
                                </div>
                                <div class="d-flex align-items-center">
                                    <div class="btn-group me-2">
                                        <a href="/Lessons/Edit/${
                                          lesson.id
                                        }" class="btn btn-sm btn-outline-primary" title="Sửa bài học">
                                            <i class="fas fa-edit"></i>
                                        </a>
                                        <a href="/Lessons/Delete/${
                                          lesson.id
                                        }" class="btn btn-sm btn-outline-danger" title="Xóa bài học">
                                            <i class="fas fa-trash"></i>
                                        </a>
                                    </div>
                                    <i class="fas fa-chevron-down"></i>
                                </div>
                            </div>
                        </div>
                        <div id="${collapseId}" class="collapse ${
          index === 0 ? "show" : ""
        }" 
                            aria-labelledby="${headingId}" data-parent="#lessonsAccordion">
                            <div class="card-body pt-0">
                                <div class="lectures-container" id="lectures-${
                                  lesson.id
                                }">
                                    <div class="text-center py-3">
                                        <i class="fas fa-spinner fa-spin"></i>
                                        <p class="mb-0">Đang tải bài giảng...</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
      });

      // Thêm lessons vào accordion (giữ lại phần loading và thông báo không có lesson)
      const loadingElement =
        document.getElementById("lessonsLoading").outerHTML;
      const noLessonsElement =
        document.getElementById("noLessonsMessage").outerHTML;
      lessonsContainer.innerHTML =
        loadingElement + noLessonsElement + lessonsHtml;

      // Ẩn thông báo loading
      document.getElementById("lessonsLoading").style.display = "none";

      // Tải lectures cho mỗi lesson
      lessons.forEach((lesson) => {
        loadLectures(lesson.id);
      });

      // Cập nhật dropdown bài học trong modal thêm bài giảng
      updateLessonDropdown(lessons);
    },
    error: function (xhr, status, error) {
      console.error("Lỗi khi tải bài học:", error);
      document.getElementById("lessonsLoading").style.display = "none";
      document.getElementById("noLessonsMessage").classList.remove("d-none");
      showToast(
        "Không thể tải bài học. Vui lòng làm mới trang và thử lại.",
        "error"
      );
    },
  });
}

/**
 * Cập nhật dropdown chọn lesson trong modal thêm bài giảng
 * @param {Array} lessons - Danh sách bài học
 */
function updateLessonDropdown(lessons) {
  const dropdown = document.getElementById("lessonSelect");
  if (!dropdown) return;

  dropdown.innerHTML = '<option value="">Chọn bài học...</option>';
  lessons.forEach((lesson) => {
    dropdown.innerHTML += `<option value="${lesson.id}">${
      lesson.title || "Bài học chưa đặt tên"
    }</option>`;
  });
}

/**
 * Tải danh sách bài giảng cho một bài học
 * @param {number} lessonId - ID của bài học
 */
function loadLectures(lessonId) {
  if (!lessonId) {
    console.error("Không tìm thấy ID bài học");
    return;
  }

  const lecturesContainer = document.getElementById(`lectures-${lessonId}`);
  if (!lecturesContainer) {
    console.error(
      "Không tìm thấy container cho bài giảng của bài học",
      lessonId
    );
    return;
  }

  // Gọi API để lấy danh sách bài giảng
  $.ajax({
    url: "/Lectures/GetLectures",
    type: "GET",
    data: { lessonId: lessonId },
    dataType: "json",
    cache: false,
    success: function (response) {
      console.log(
        "Kết quả API GetLectures cho bài học",
        lessonId,
        ":",
        response
      );

      // Xử lý lỗi nếu có
      if (!response.success) {
        lecturesContainer.innerHTML = `
                    <div class="alert alert-warning">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        ${response.message || "Không thể tải bài giảng"}
                        <button class="btn btn-sm btn-outline-warning mt-2" onclick="loadLectures(${lessonId})">
                            <i class="fas fa-sync-alt me-1"></i> Thử lại
                        </button>
                    </div>
                `;
        return;
      }

      // Tạo nội dung HTML
      let html = `
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <h6 class="mb-0 text-muted">Bài giảng</h6>
                    <button class="btn btn-sm btn-outline-primary" onclick="event.stopPropagation();" 
                        data-toggle="modal" data-target="#lectureModal" 
                        data-lesson-id="${lessonId}">
                        <i class="fas fa-plus me-1"></i> Thêm bài giảng
                    </button>
                </div>
            `;

      // Lấy dữ liệu lectures
      const lectures = response.data || [];

      // Hiển thị thông báo nếu không có bài giảng
      if (lectures.length === 0) {
        html += `
                    <div class="text-center py-4 text-muted">
                        <i class="fas fa-file-video fa-3x mb-3 opacity-50"></i>
                        <p>Chưa có bài giảng nào trong bài học này</p>
                    </div>
                `;
        lecturesContainer.innerHTML = html;
        return;
      }

      // Tạo danh sách bài giảng
      html += '<ul class="list-group list-group-flush">';

      lectures.forEach((lecture) => {
        // Xác định icon và loại nội dung
        let iconClass = "fas fa-file-alt";
        let iconColorClass = "text-primary";
        let contentTypeText = "Văn bản";
        let actionButton = "";

        if (lecture.videoUrl) {
          iconClass = "fas fa-play-circle";
          iconColorClass = "text-info";
          contentTypeText = "Video";
          actionButton = `<button class="btn btn-sm btn-primary" title="Xem" onclick="event.stopPropagation(); playVideo('${lecture.videoUrl}', '${lecture.title}')">
                                        <i class="fas fa-play"></i>
                                    </button>`;
        }

        // Tạo HTML cho mỗi bài giảng
        html += `
                    <li class="list-group-item px-0 py-3 border-0 border-bottom" data-lecture-id="${
                      lecture.id
                    }">
                        <div class="d-flex align-items-center">
                            <div class="me-3 ${iconColorClass}">
                                <i class="${iconClass} fa-lg"></i>
                            </div>
                            <div class="flex-grow-1">
                                <h6 class="mb-0">${
                                  lecture.title || "Bài giảng chưa đặt tên"
                                }</h6>
                                <div class="d-flex justify-content-between align-items-center">
                                    <small class="text-muted">${contentTypeText} • ${
          lecture.durationMinutes || 0
        } phút</small>
                                    <div>
                                        <a href="/Lectures/Edit/${
                                          lecture.id
                                        }" class="btn btn-sm btn-outline-primary me-1" title="Sửa bài giảng">
                                            <i class="fas fa-edit"></i>
                                        </a>
                                        <a href="/Lectures/Delete/${
                                          lecture.id
                                        }" class="btn btn-sm btn-outline-danger me-1" title="Xóa bài giảng">
                                            <i class="fas fa-trash"></i>
                                        </a>
                                        ${actionButton}
                                    </div>
                                </div>
                            </div>
                        </div>
                    </li>
                `;
      });

      html += "</ul>";
      lecturesContainer.innerHTML = html;
    },
    error: function (xhr, status, error) {
      console.error("Lỗi khi tải bài giảng cho bài học", lessonId, ":", error);
      lecturesContainer.innerHTML = `
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    Không thể tải bài giảng. Lỗi kết nối.
                    <button class="btn btn-sm btn-outline-danger mt-2" onclick="loadLectures(${lessonId})">
                        <i class="fas fa-sync-alt me-1"></i> Thử lại
                    </button>
                </div>
            `;
    },
  });
}

/**
 * Hiển thị video trong modal
 * @param {string} videoUrl - URL của video
 * @param {string} title - Tiêu đề video
 */
function playVideo(videoUrl, title = "Bài giảng") {
  if (!videoUrl) {
    showToast("Không có URL video hợp lệ", "error");
    return;
  }

  // Kiểm tra xem modal đã tồn tại chưa
  let videoModal = document.getElementById("videoPlayerModal");
  if (!videoModal) {
    // Tạo modal nếu chưa tồn tại
    videoModal = document.createElement("div");
    videoModal.id = "videoPlayerModal";
    videoModal.className = "modal fade";
    videoModal.setAttribute("tabindex", "-1");
    videoModal.setAttribute("role", "dialog");
    videoModal.setAttribute("aria-labelledby", "videoPlayerModalLabel");
    videoModal.setAttribute("aria-hidden", "true");

    videoModal.innerHTML = `
            <div class="modal-dialog modal-lg modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="videoTitle">${title}</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close" onclick="stopVideo()">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body p-0">
                        <iframe id="videoFrame" width="100%" height="500" src="" frameborder="0" 
                            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" 
                            allowfullscreen></iframe>
                    </div>
                </div>
            </div>
        `;

    document.body.appendChild(videoModal);
  }

  // Cập nhật nội dung modal
  document.getElementById("videoTitle").textContent = title;
  document.getElementById("videoFrame").src = videoUrl;

  // Hiển thị modal
  $("#videoPlayerModal").modal("show");
}

/**
 * Dừng phát video khi đóng modal
 */
function stopVideo() {
  const iframe = document.getElementById("videoFrame");
  if (iframe) iframe.src = "";
}

/**
 * Hiển thị thông báo toast
 * @param {string} message - Nội dung thông báo
 * @param {string} type - Loại thông báo (success, error, warning, info)
 * @param {number} duration - Thời gian hiển thị (ms)
 */
function showToast(message, type = "info", duration = 5000) {
  // Kiểm tra xem container đã tồn tại chưa
  let toastContainer = document.getElementById("toastContainer");
  if (!toastContainer) {
    toastContainer = document.createElement("div");
    toastContainer.id = "toastContainer";
    toastContainer.className = "position-fixed top-0 end-0 p-3";
    toastContainer.style.zIndex = "1050";
    document.body.appendChild(toastContainer);
  }

  // Tạo toast element
  const toastId = "toast-" + new Date().getTime();
  const toast = document.createElement("div");
  toast.id = toastId;
  toast.className = `toast bg-${type} text-white`;
  toast.setAttribute("role", "alert");
  toast.setAttribute("aria-live", "assertive");
  toast.setAttribute("aria-atomic", "true");

  // Đặt nội dung toast
  toast.innerHTML = `
        <div class="toast-header bg-${type} text-white">
            <strong class="me-auto">${
              type === "error"
                ? "Lỗi"
                : type === "success"
                ? "Thành công"
                : "Thông báo"
            }</strong>
            <button type="button" class="btn-close btn-close-white" data-dismiss="toast" aria-label="Close"></button>
        </div>
        <div class="toast-body">
            ${message}
        </div>
    `;

  // Thêm toast vào container
  toastContainer.appendChild(toast);

  // Hiển thị toast
  $(toast)
    .toast({
      delay: duration,
      autohide: true,
    })
    .toast("show");

  // Xóa toast sau khi ẩn
  $(toast).on("hidden.bs.toast", function () {
    toast.remove();
  });
}

// Khởi tạo khi tài liệu đã sẵn sàng
$(document).ready(function () {
  // Tải lesson khi tab lecture được kích hoạt
  $("#lecture-tab").on("click", function () {
    const classRoomId = document.querySelector(
      'input[name="ClassRoomId"]'
    ).value;
    loadLessons(classRoomId);
  });

  // Khởi tạo nếu tab lecture đang active khi tải trang
  if (
    $("#lecture-tab").hasClass("active") ||
    $("#lecture").hasClass("active show")
  ) {
    const classRoomId = document.querySelector(
      'input[name="ClassRoomId"]'
    ).value;
    loadLessons(classRoomId);
  }

  // Xử lý sự kiện đóng modal video
  $(document).on("hidden.bs.modal", "#videoPlayerModal", function () {
    stopVideo();
  });

  // Xử lý sự kiện khi modal thêm bài giảng hiển thị
  $("#lectureModal").on("show.bs.modal", function (event) {
    const button = $(event.relatedTarget);
    const lessonId = button.data("lesson-id");

    if (lessonId) {
      $("#lessonSelect").val(lessonId);
    }

    // Hiển thị section tương ứng với loại content được chọn
    toggleContentType();
  });
});

/**
 * Chuyển đổi hiển thị giữa các loại nội dung bài giảng
 */
function toggleContentType() {
  // Ẩn tất cả các section
  document.querySelectorAll(".content-type-section").forEach((section) => {
    section.style.display = "none";
  });

  // Hiển thị section được chọn
  const contentType = document.querySelector(
    'input[name="contentType"]:checked'
  ).value;

  if (contentType === "videoUrl") {
    document.getElementById("videoUrlContent").style.display = "block";
  } else if (contentType === "upload") {
    document.getElementById("uploadContent").style.display = "block";
  } else if (contentType === "text") {
    document.getElementById("textContent").style.display = "block";
  }
}
