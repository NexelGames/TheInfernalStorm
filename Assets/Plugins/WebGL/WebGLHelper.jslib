var WebGLHelper = {
   IsMobile: function()
   {
      return Module.SystemInfo.mobile;
   }
};  
mergeInto(LibraryManager.library, WebGLHelper);