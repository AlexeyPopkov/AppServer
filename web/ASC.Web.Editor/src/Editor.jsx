import React, { useEffect, useState } from "react";
import Toast from "@appserver/components/toast";
import toastr from "@appserver/components/toast/toastr";
import Box from "@appserver/components/box";
import { regDesktop } from "@appserver/common/desktop";
import Loaders from "@appserver/common/components/Loaders";
import {
  combineUrl,
  getObjectByLocation,
  //showLoader,
  //hideLoader,
  tryRedirectTo,
} from "@appserver/common/utils";
import {
  getDocServiceUrl,
  openEdit,
  setEncryptionKeys,
  getEncryptionAccess,
} from "@appserver/common/api/files";
import { checkIsAuthenticated } from "@appserver/common/api/user";
import { getUser } from "@appserver/common/api/people";
import throttle from "lodash/throttle";
import { isIOS, deviceType } from "react-device-detect";
import { homepage } from "../package.json";
import "./custom.scss";
import { AppServerConfig } from "@appserver/common/constants";

let documentIsReady = false;

let docTitle = null;
let fileType = null;

let docSaved = null;

const Editor = () => {
  const urlParams = getObjectByLocation(window.location);
  const fileId = urlParams
    ? urlParams.fileId || urlParams.fileid || null
    : null;
  const doc = urlParams ? urlParams.doc || null : null;
  const isDesktop = window["AscDesktopEditor"] !== undefined;

  const [isLoading, setIsLoading] = useState(true);
  const [isAuthenticated, setIsAuthenticated] = useState(true);

  const throttledChangeTitle = throttle(
    () => changeTitle(docSaved, docTitle),
    500
  );

  const init = async () => {
    try {
      if (!fileId) return;

      console.log("PureEditor componentDidMount", fileId, doc);

      if (isIPad()) {
        const vh = window.innerHeight * 0.01;
        document.documentElement.style.setProperty("--vh", `${vh}px`);
      }

      //showLoader();

      const docApiUrl = await getDocServiceUrl();

      if (!doc) {
        const success = await checkIsAuthenticated();

        if (!success) {
          return tryRedirectTo(combineUrl(AppServerConfig.proxyURL, "/login"));
        } else {
          setIsAuthenticated(success);
        }
      }

      const config = await openEdit(fileId, doc);

      if (isDesktop) {
        const isEncryption =
          config.editorConfig["encryptionKeys"] !== undefined;
        const user = await getUser();

        regDesktop(
          user,
          isEncryption,
          config.editorConfig.encryptionKeys,
          (keys) => {
            setEncryptionKeys(keys);
          },
          true,
          (callback) => {
            getEncryptionAccess(fileId)
              .then((keys) => {
                var data = {
                  keys,
                };

                callback(data);
              })
              .catch((error) => {
                console.log(error);
                toastr.error(
                  typeof error === "string" ? error : error.message,
                  null,
                  0,
                  true
                );
              });
          }
        );
      }

      setIsLoading(false);

      loadDocApi(docApiUrl, () => onLoad(config));
    } catch (error) {
      console.log(error);
      toastr.error(
        typeof error === "string" ? error : error.message,
        null,
        0,
        true
      );
    }
  };

  useEffect(() => {
    init();
  }, []);

  const isIPad = () => {
    return isIOS && deviceType === "tablet";
  };

  const setFavicon = (fileType) => {
    const favicon = document.getElementById("favicon");
    if (!favicon) return;
    let icon = null;
    switch (fileType) {
      case "docx":
        icon = "text.ico";
        break;
      case "pptx":
        icon = "presentation.ico";
        break;
      case "xlsx":
        icon = "spreadsheet.ico";
        break;
      default:
        break;
    }

    if (icon) favicon.href = `${homepage}/images/${icon}`;
  };

  const changeTitle = (docSaved, docTitle) => {
    docSaved ? setDocumentTitle(docTitle) : setDocumentTitle(`*${docTitle}`);
  };

  const setDocumentTitle = (subTitle = null) => {
    //const { isAuthenticated, settingsStore, product: currentModule } = auth;
    //const { organizationName } = settingsStore;
    const organizationName = "ONLYOFFICE"; //TODO: Replace to API variant
    const moduleTitle = "Documents"; //TODO: Replace to API variant

    let title;
    if (subTitle) {
      if (isAuthenticated && moduleTitle) {
        title = subTitle + " - " + moduleTitle;
      } else {
        title = subTitle + " - " + organizationName;
      }
    } else if (moduleTitle && organizationName) {
      title = moduleTitle + " - " + organizationName;
    } else {
      title = organizationName;
    }

    document.title = title;
  };

  const loadDocApi = (docApiUrl, onLoadCallback) => {
    const script = document.createElement("script");
    script.setAttribute("type", "text/javascript");
    script.setAttribute("id", "scripDocServiceAddress");

    script.onload = onLoadCallback;

    script.src = docApiUrl;
    script.async = true;

    console.log("PureEditor componentDidMount: added script");
    document.body.appendChild(script);
  };

  const onLoad = (config) => {
    console.log("Editor config: ", config);
    try {
      console.log(config);

      docTitle = config.document.title;
      fileType = config.document.fileType;

      setFavicon(fileType);
      setDocumentTitle(docTitle);

      if (window.innerWidth < 720) {
        config.type = "mobile";
      }

      const events = {
        events: {
          onAppReady: onSDKAppReady,
          onDocumentStateChange: onDocumentStateChange,
          onMetaChange: onMetaChange,
          onDocumentReady: onDocumentReady,
          onInfo: onSDKInfo,
          onWarning: onSDKWarning,
          onError: onSDKError,
        },
      };

      const newConfig = Object.assign(config, events);

      if (!window.DocsAPI) throw new Error("DocsAPI is not defined");

      //hideLoader();

      window.DocsAPI.DocEditor("editor", newConfig);
    } catch (error) {
      console.log(error);
      toastr.error(error.message, null, 0, true);
    }
  };

  const onSDKAppReady = () => {
    console.log("ONLYOFFICE Document Editor is ready");
  };

  const onSDKInfo = (event) => {
    console.log(
      "ONLYOFFICE Document Editor is opened in mode " + event.data.mode
    );
  };

  const onSDKWarning = (event) => {
    console.log(
      "ONLYOFFICE Document Editor reports a warning: code " +
        event.data.warningCode +
        ", description " +
        event.data.warningDescription
    );
  };

  const onSDKError = (event) => {
    console.log(
      "ONLYOFFICE Document Editor reports an error: code " +
        event.data.errorCode +
        ", description " +
        event.data.errorDescription
    );
  };

  const onDocumentStateChange = (event) => {
    if (!documentIsReady) return;

    docSaved = !event.data;
    throttledChangeTitle();
  };

  const onDocumentReady = () => {
    documentIsReady = true;
  };

  const onMetaChange = (event) => {
    const newTitle = event.data.title;
    if (newTitle && newTitle !== docTitle) {
      setDocumentTitle(newTitle);
      docTitle = newTitle;
    }
  };

  return (
    <Box
      widthProp="100vw"
      heightProp={isIPad() ? "calc(var(--vh, 1vh) * 100)" : "100vh"}
    >
      <Toast />

      {!isLoading ? (
        <div id="editor"></div>
      ) : (
        <Box paddingProp="16px">
          <Loaders.Rectangle height="96vh" />
        </Box>
      )}
    </Box>
  );
};

export default Editor;
