import React, { lazy, Suspense } from "react";
import { Route } from "react-router-dom";
import Home from "./Home";

const routes = (
    <Route path="/">
        <Route index element={<Suspense fallback={<div>Loading...</div>}><Home /></Suspense>} />
        <Route path="about" element={<Suspense fallback={<div>Loading...</div>}>About</Suspense>} />
        <Route path="contact" element={<Suspense fallback={<div>Loading...</div>}>Contact</Suspense>} />
    </Route>
);

export default routes;
