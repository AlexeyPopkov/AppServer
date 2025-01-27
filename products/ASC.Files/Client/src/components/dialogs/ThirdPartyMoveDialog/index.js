import React from "react";
import styled from "styled-components";

import { withTranslation } from "react-i18next";
import ModalDialog from "@appserver/components/modal-dialog";
import Text from "@appserver/components/text";
import Button from "@appserver/components/button";
import { inject, observer } from "mobx-react";

const StyledOperationDialog = styled.div`
  .operation-button {
    margin-right: 8px;
  }
`;

const PureThirdPartyMoveContainer = ({
  t,
  visible,
  startMoveOperation,
  startCopyOperation,
  provider,
  setThirdPartyMoveDialogVisible,
}) => {
  const zIndex = 310;

  const onClose = () => setThirdPartyMoveDialogVisible(false);

  return (
    <StyledOperationDialog>
      <ModalDialog visible={visible} zIndex={zIndex} onClose={onClose}>
        <ModalDialog.Header>{t("MoveConfirmation")}</ModalDialog.Header>
        <ModalDialog.Body>
          <Text>{t("MoveConfirmationMessage", { provider })}</Text>
          <br />
          <Text>{t("MoveConfirmationAlert")}</Text>
        </ModalDialog.Body>

        <ModalDialog.Footer>
          <Button
            className="operation-button"
            label={t("Move")}
            size="big"
            primary
            onClick={startMoveOperation}
          />
          <Button
            className="operation-button"
            label={t("Copy")}
            size="big"
            onClick={startCopyOperation}
          />
          <Button
            className="operation-button"
            label={t("CancelButton")}
            size="big"
            onClick={onClose}
          />
        </ModalDialog.Footer>
      </ModalDialog>
    </StyledOperationDialog>
  );
};

export default inject(({ filesStore, dialogsStore }) => {
  const {
    thirdPartyMoveDialogVisible: visible,
    setThirdPartyMoveDialogVisible,
  } = dialogsStore;
  const { selection } = filesStore;

  return {
    visible,
    setThirdPartyMoveDialogVisible,
    provider: selection[0].providerKey,
  };
})(
  withTranslation("ThirdPartyMoveDialog")(observer(PureThirdPartyMoveContainer))
);
