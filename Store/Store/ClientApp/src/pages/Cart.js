import React, { useState, useEffect } from "react";
import { Box, Grid, Link, Stack, Divider, Button, Dialog } from "@mui/material";
import { Typography } from "@mui/material";
import { styled } from "@mui/styles";
import CartItem, { CartItemsHeader } from "../components/CartItem";
import { get, post } from "../utils/fetchWrapper";
import AddressDialog from "../components/AddressDialog";

const Content = styled(Box)(({ theme }) => ({
  alignSelf: "center",
  padding: theme.spacing(0, 32),
  margin: theme.spacing(18, 0),
}));

const Cart = () => {
  const [cartItems, setCartItems] = useState([]);
  const [total, setTotal] = useState(0);
  const [modalOpen, setModalOpen] = useState(false);

  const onCountInc = (cartItemModel) => {
    post("api/Cart/ChangeItemCount", console.log, cartItemModel);
  };
  const onCountDec = (cartItemModel) => {
    post("api/Cart/ChangeItemCount", console.log, cartItemModel);
  };
  const onRemove = (cartItemModel) => {
    post("api/Cart/Remove", console.log, cartItemModel);
    console.log(cartItemModel);
  };

  const handleCheckoutConfirm = (address) => {
    console.log(address);
  };

  useEffect(() => {
    get("api/Cart/GetShoppingDetails", setCartItems);
    get("api/Cart/GetTotal", setTotal);
  }, []);

  return (
    <>
      <AddressDialog
        open={modalOpen}
        handleClose={() => setModalOpen(false)}
        onCheckoutConfirm={handleCheckoutConfirm}
      />
      <Content>
        <Grid
          container
          spacing={6}
          direction="column"
          justifyContent="center"
          alignItems="center"
          wrap="nowrap"
        >
          <Grid item>
            <Typography variant="h1" color="initial">
              Your cart items
            </Typography>
          </Grid>
          <Grid item>
            <Link href="">
              <Typography variant="h2" color="primary">
                Continue shopping
              </Typography>
            </Link>
          </Grid>
          <Grid item style={{ width: "100%" }}>
            <Stack divider={<Divider flexItem />} spacing={2}>
              <CartItemsHeader />
              {cartItems.length ? (
                cartItems.map((item) => (
                  <CartItem
                    id={item.itemID}
                    count={item.count}
                    onCountInc={onCountInc}
                    onCountDec={onCountDec}
                    onRemove={onRemove}
                    key={item.itemID}
                  />
                ))
              ) : (
                <Box padding="20vh">
                  <Typography variant="h1" color="primary" textAlign="center">
                    Cart is empty
                  </Typography>
                </Box>
              )}
            </Stack>
          </Grid>
          {cartItems.length > 0 && (
            <Grid
              item
              container
              justifyContent="flex-end"
              alignItems="center"
              spacing={10}
            >
              <Grid item>
                <Typography variant="h2" color="initial" textAlign="right">
                  Subtotal:
                </Typography>
              </Grid>
              <Grid item>
                <Typography variant="h1" color="initial" textAlign="right">
                  {total ? `$ ${total}` : ""}
                </Typography>
              </Grid>
              <Grid item>
                <Button size="medium" onClick={() => setModalOpen(true)}>
                  <Typography variant="h2" color="white">
                    Check-Out
                  </Typography>
                </Button>
              </Grid>
            </Grid>
          )}
        </Grid>
      </Content>
    </>
  );
};

export default Cart;
